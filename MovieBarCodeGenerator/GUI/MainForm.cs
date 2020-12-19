//Copyright 2011-2018 Melvyn Laily
//https://zerowidthjoiner.net

//This file is part of MovieBarCodeGenerator.

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program.  If not, see <http://www.gnu.org/licenses/>.

using MovieBarCodeGenerator.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieBarCodeGenerator.GUI
{
    public partial class MainForm : Form
    {
        private const string GenerateButtonText = "Generate!";
        private const string CancelButtonText = "Cancel";

        private OpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;
        private FfmpegWrapper _ffmpegWrapper;
        private ImageStreamProcessor _imageProcessor;
        private BarCodeParametersValidator _barCodeParametersValidator;

        private CancellationTokenSource _cancellationTokenSource;

        public MainForm()
        {
            InitializeComponent();

            var executingAssembly = Assembly.GetExecutingAssembly();
            Icon = Icon.ExtractAssociatedIcon(executingAssembly.Location);
            Text += $" - {executingAssembly.GetName().Version}";

            AppendLog(Text);

            _openFileDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
            };

            _saveFileDialog = new SaveFileDialog()
            {
                DefaultExt = ".png",
                Filter = "Bitmap|*.bmp|Jpeg|*.jpg|Png|*.png|Gif|*.gif|All files|*.*",
                FilterIndex = 3, // 1 based
                OverwritePrompt = true,
            };

            _ffmpegWrapper = new FfmpegWrapper("ffmpeg.exe");
            _imageProcessor = new ImageStreamProcessor();
            _barCodeParametersValidator = new BarCodeParametersValidator();

            useInputHeightForOutputCheckBox.Checked = true;
            generateButton.Text = GenerateButtonText;
        }

        private async void generateButton_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                generateButton.Text = GenerateButtonText;
                progressBar1.Value = progressBar1.Minimum;
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
                return;
            }

            // Validate parameters:

            bool PromptOverwriteExistingOutputFile(string path)
            {
                var promptResult = MessageBox.Show(this,
                     $"The file '{path}' already exists. Do you want to overwrite it?",
                     "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                return promptResult == DialogResult.Yes;
            }

            CompleteBarCodeGenerationParameters parameters;
            try
            {
                parameters = _barCodeParametersValidator.GetValidatedParameters(
                    rawInputPath: inputPathTextBox.Text,
                    rawOutputPath: outputPathTextBox.Text,
                    rawBarWidth: barWidthTextBox.Text,
                    rawImageWidth: imageWidthTextBox.Text,
                    rawImageHeight: imageHeightTextBox.Text,
                    useInputHeightForOutput: useInputHeightForOutputCheckBox.Checked,
                    generateSmoothVersion: smoothCheckBox.Checked,
                    shouldOverwriteOutput: PromptOverwriteExistingOutputFile);
            }
            catch (OperationCanceledException)
            {
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
                return;
            }
            catch (Exception ex)
            {
                AppendLog("Error validating input parameters. " + ex.ToString());
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            AppendLog($@"Barcode generation starting...
Input: {parameters.InputPath}
Output: {parameters.OutputPath}
Output width: {parameters.BarCode.Width}
Output height: {parameters.BarCode.Height}
Bar width: {parameters.BarCode.BarWidth}");

            // Register progression callback and ready cancellation source:

            var progress = new PercentageProgressHandler(percentage =>
            {
                var progressBarValue = Math.Min(100, (int)Math.Round(percentage * 100, MidpointRounding.AwayFromZero));
                Invoke(new Action(() =>
                {
                    if (_cancellationTokenSource != null)
                    {
                        progressBar1.Value = progressBarValue;
                        TaskbarProgress.SetValue(Handle, progressBarValue, 100);
                    }
                }));
            });

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationLocalRef = _cancellationTokenSource;

            // Actually create the barcode:

            var gdiBarGenerator = new GdiBarGenerator(smoothed: false);
            var smoothedBarGenerator = new GdiBarGenerator(smoothed: true);

            var generators = new List<IBarGenerator> { gdiBarGenerator };
            if (parameters.GenerateSmoothedOutput)
            {
                generators.Add(smoothedBarGenerator);
            }

            IReadOnlyDictionary<IBarGenerator, Bitmap> result = null;
            try
            {
                generateButton.Text = CancelButtonText;
                generateButton.Enabled = false;
                // Prevent the user from cancelling for 1sec (it might not be obvious the generation has started)
                var dontCare = Task.Delay(1000).ContinueWith(t =>
                {
                    try
                    {
                        Invoke(new Action(() => generateButton.Enabled = true));
                    }
                    catch { }
                });

                await Task.Run(() =>
                {
                    result = _imageProcessor.CreateBarCode(
                        parameters.InputPath,
                        parameters.BarCode,
                        _ffmpegWrapper,
                        _cancellationTokenSource.Token,
                        progress,
                        AppendLog,
                        generators.ToArray());
                }, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                AppendLog("Operation cancelled.");
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
                return;
            }
            catch (Exception ex)
            {
                AppendLog("Error: " + ex.ToString());
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
                MessageBox.Show(this, "Sorry, something went wrong. See the log for more info.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                generateButton.Text = GenerateButtonText;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }

            if (cancellationLocalRef.IsCancellationRequested)
            {
                AppendLog("Operation cancelled.");
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
                return;
            }

            // Save the barcode:

            AppendLog("Saving the image...");

            try
            {
                result[gdiBarGenerator].Save(parameters.OutputPath);
            }
            catch (Exception ex)
            {
                var message = $"Unable to save the image: {ex}";
                AppendLog(message);
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
                MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (parameters.GenerateSmoothedOutput)
            {
                try
                {
                    result[smoothedBarGenerator].Save(parameters.SmoothedOutputPath);
                }
                catch (Exception ex)
                {
                    var message = $"Unable to save the smoothed image: {ex}";
                    AppendLog(message);
                    TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
                    MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            AppendLog("Barcode generated successfully!");
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
        }

        private void browseInputPathButton_Click(object sender, EventArgs e)
        {
            if (_openFileDialog.ShowDialog(owner: this) == DialogResult.OK)
            {
                inputPathTextBox.Text = _openFileDialog.FileName;
            }
        }

        private void browseOutputPathButton_Click(object sender, EventArgs e)
        {
            if (_saveFileDialog.ShowDialog(owner: this) == DialogResult.OK)
            {
                outputPathTextBox.Text = _saveFileDialog.FileName;
            }
        }

        private void useInputHeightForOutputCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            imageHeightTextBox.ReadOnly = useInputHeightForOutputCheckBox.Checked;
        }

        private void imageWidthTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (int.TryParse(imageWidthTextBox.Text, out var imageWidth)
             && int.TryParse(barWidthTextBox.Text, out var barWidth))
            {
                try
                {
                    var newBarCount = (int)Math.Round((double)imageWidth / barWidth);
                    barCountTextBox.Text = newBarCount.ToString();
                }
                catch { }
            }
        }

        private void barCountTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (int.TryParse(barCountTextBox.Text, out var barCount)
                && int.TryParse(imageWidthTextBox.Text, out var imageWidth))
            {
                try
                {
                    var newBarWidth = (int)Math.Round((double)imageWidth / barCount);
                    barWidthTextBox.Text = newBarWidth.ToString();
                }
                catch { }
            }
        }

        private void barWidthTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (int.TryParse(barWidthTextBox.Text, out var barWidth)
                && int.TryParse(imageWidthTextBox.Text, out var imageWidth))
            {
                try
                {
                    var newBarCount = (int)Math.Round((double)imageWidth / barWidth);
                    barCountTextBox.Text = newBarCount.ToString();
                }
                catch { }
            }
        }

        private void aboutButton_Click(object sender, EventArgs e) => new AboutBox().ShowDialog();

        private void AppendLog(string value)
        {
            if (value == null)
            {
                return;
            }

            if (logTextBox.InvokeRequired)
            {
                logTextBox.Invoke(new Action(() => AppendLog(value)));
                return;
            }

            logTextBox.AppendText($"{DateTime.Now:u} - " + value + Environment.NewLine);
        }

        private void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (sender is TextBox target)
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                    var path = paths.FirstOrDefault();
                    if (path != null)
                    {
                        target.Text = path;
                    }
                }
                else if (e.Data.GetDataPresent(DataFormats.Text))
                {
                    target.Text = (string)e.Data.GetData(DataFormats.Text);
                }
            }
        }

        private void TextBox_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) || e.Data.GetDataPresent(DataFormats.Text))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
    }

    public class PercentageProgressHandler : IProgress<double>
    {
        private readonly Action<double> _handler;

        public PercentageProgressHandler(Action<double> handler)
        {
            _handler = handler;
        }
        public void Report(double value) => _handler(value);
    }
}
