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

using Microsoft.WindowsAPICodePack.Dialogs;
using MovieBarCodeGenerator.Core;
using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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

        private CommonOpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;
        private ImageProvider _imageProvider;
        private ImageStreamProcessor _imageProcessor;
        private BarCodeParametersValidator _barCodeParametersValidator;

        private CancellationTokenSource _cancellationTokenSource;

        private List<BarGeneratorViewModel> _barGenerators;

        public MainForm()
        {
            InitializeComponent();

            var executingAssembly = Assembly.GetExecutingAssembly();
            Icon = Icon.ExtractAssociatedIcon(executingAssembly.Location);
            Text += $" - {executingAssembly.GetName().Version}-beta - This version uses an image folder instead of a video!";

            _barGenerators = new List<BarGeneratorViewModel>
            {
                new BarGeneratorViewModel(
                    new MagicScalerBarGenerator("Normal", average: false),
                    "The default mode to generate barcodes.\r\nIt scales images using a resampling algorithm that takes care of gamma correction and produces correct color averages.",
                    initialCheckState: true),
                new BarGeneratorViewModel(
                    new MagicScalerBarGenerator("Normal (smoothed)", "_smoothed", average: true, InterpolationSettings.CubicSmoother),
                    "Almost the same as the 'Normal' mode, but vertically smoothed.\r\nIt also uses a 'cubic smoother' resampling algorithm that generates images sharper than the normal algorithm.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    GdiBarGenerator.CreateLegacy(average: false),
                    "The mode used in previous versions.\r\nIt's relatively fast, but the algorithm used to scale images is of poor quality.\r\nThis mode is not recommended, and only here for retro-compatibility.",
                    initialCheckState: false),
                new BarGeneratorViewModel(
                    GdiBarGenerator.CreateLegacy(average: true),
                    "Same as 'Legacy', but vertically smoothed.",
                    initialCheckState: false),
            };

            barGeneratorList.DisplayMember = nameof(BarGeneratorViewModel.DisplayName);
            barGeneratorList.Items.Clear();
            foreach (var item in _barGenerators)
                barGeneratorList.Items.Add(item, isChecked: item.Checked);

            barGeneratorList.SelectedItem = _barGenerators.First(x => x.Checked); // So that the right panel displays something.
            barGeneratorList.SelectedItem = null; // Unselect so a click on the line will not uncheck the item.

            AppendLog(Text);

            _openFileDialog = new CommonOpenFileDialog()
            {
                EnsureFileExists = true,
                EnsurePathExists = true,
                IsFolderPicker = true,
            };

            _saveFileDialog = new SaveFileDialog()
            {
                DefaultExt = ".png",
                Filter = "Bitmap|*.bmp|Jpeg|*.jpg|Png|*.png|Gif|*.gif|All files|*.*",
                FilterIndex = 3, // 1 based
                OverwritePrompt = true,
            };

            _imageProvider = new ImageProvider();
            _imageProcessor = new ImageStreamProcessor();
            _barCodeParametersValidator = new BarCodeParametersValidator();

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

            bool PromptOverwriteExistingOutputFile(IReadOnlyCollection<string> paths)
            {
                var promptResult = MessageBox.Show(this,
                     $"The following files already exist: '{string.Join(", ", paths.Select(x => $"'{x}'"))}'. Do you want to overwrite them?",
                     "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                return promptResult == DialogResult.Yes;
            }

            var generators =
                _barGenerators
                .Where(x => x.Checked)
                .Select(x => x.Generator)
                .ToArray();

            if (generators.Any() == false)
            {
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
                MessageBox.Show(this, "At least one barcode version must be selected.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            BarCodeParameters parameters;
            try
            {
                parameters = _barCodeParametersValidator.GetValidatedParameters(
                    rawInputPath: inputPathTextBox.Text,
                    rawBaseOutputPath: outputPathTextBox.Text,
                    rawBarWidth: barWidthTextBox.Text,
                    rawImageWidth: imageWidthTextBox.Text,
                    rawImageHeight: imageHeightTextBox.Text,
                    shouldOverwriteOutputPaths: PromptOverwriteExistingOutputFile,
                    barGenerators: generators);
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
Input: '{parameters.InputPath}'
Output: {string.Join(", ", parameters.GeneratorOutputPaths.Select(x => $"'{x.Value}'"))}
Output width: {parameters.Width}
Output height: {parameters.Height}
Bar width: {parameters.BarWidth}");

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
                    result = _imageProcessor.CreateBarCodes(
                        parameters,
                        _imageProvider,
                        _cancellationTokenSource.Token,
                        progress,
                        AppendLog);
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

            AppendLog("Saving the images...");

            try
            {
                foreach (var barcode in result)
                {
                    var outputPath = parameters.GeneratorOutputPaths[barcode.Key];
                    barcode.Value.Save(outputPath);
                }
            }
            catch (Exception ex)
            {
                var message = $"Unable to save the images: {ex}";
                AppendLog(message);
                TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.Error);
                MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            AppendLog("Barcode generated successfully!");
            TaskbarProgress.SetState(Handle, TaskbarProgress.TaskbarStates.NoProgress);
        }

        private void browseInputPathButton_Click(object sender, EventArgs e)
        {
            if (_openFileDialog.ShowDialog(this.Handle) == CommonFileDialogResult.Ok)
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

        private void barGeneratorList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (barGeneratorList.Items[e.Index] is BarGeneratorViewModel generator)
            {
                generator.Checked = e.NewValue == CheckState.Checked ? true : false;
            }
        }

        private void barGeneratorList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (barGeneratorList.SelectedItem is BarGeneratorViewModel generator)
            {
                generatorInfoBody.Text = $"{generator.DisplayName}\r\n{generator.Details}";
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

    public class BarGeneratorViewModel
    {
        public IBarGenerator Generator { get; }
        public string Details { get; }

        public bool Checked { get; set; }
        public string DisplayName => Generator.DisplayName;

        public BarGeneratorViewModel(IBarGenerator generator, string details, bool initialCheckState)
        {
            Generator = generator;
            Details = details;
            Checked = initialCheckState;
        }
    }
}
