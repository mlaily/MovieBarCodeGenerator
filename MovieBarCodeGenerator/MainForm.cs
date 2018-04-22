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

namespace MovieBarCodeGenerator
{
    public partial class MainForm : Form
    {
        private const string GenerateButtonText = "Generate!";
        private const string CancelButtonText = "Cancel";

        private OpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;
        private FfmpegWrapper _ffmpegWrapper;
        private ImageProcessor _imageProcessor;

        private CancellationTokenSource _cancellationTokenSource;

        public MainForm()
        {
            InitializeComponent();

            var executingAssembly = Assembly.GetExecutingAssembly();
            Icon = Icon.ExtractAssociatedIcon(executingAssembly.Location);
            Text += $" - {executingAssembly.GetName().Version}";

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
            _imageProcessor = new ImageProcessor();

            useInputHeightForOutputCheckBox.Checked = true;
            generateButton.Text = GenerateButtonText;
        }

        private async void generateButton_Click(object sender, EventArgs e)
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
                generateButton.Text = GenerateButtonText;
                progressBar1.Value = progressBar1.Minimum;
                return;
            }

            // Validate parameters:

            string inputPath;
            string outputPath;
            BarCodeParameters parameters;
            try
            {
                (inputPath, outputPath, parameters) = GetValidatedParameters();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Register progression callback and ready cancellation source:

            var progress = new PercentageProgressHandler(percentage =>
            {
                var progressBarValue = Math.Min(100, (int)Math.Round(percentage * 100, MidpointRounding.AwayFromZero));
                Invoke(new Action(() => progressBar1.Value = progressBarValue));
            });

            _cancellationTokenSource = new CancellationTokenSource();

            // Actually create the barcode:

            Bitmap result = null;
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
                    result = _imageProcessor.CreateBarCode(inputPath, parameters, _ffmpegWrapper, _cancellationTokenSource.Token, progress);
                });
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    $@"Sorry, something went wrong, and it probably won't resolve on its own.
Here is all the info available at the time of the error (press Ctrl+C to copy it).

Input: {inputPath}
Output width: {parameters.Width}
Output height: {parameters.Height}
Bar width: {parameters.BarWidth}
Smoothen: {parameters.Smoothen}
Error: {ex}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                generateButton.Text = GenerateButtonText;
            }

            // Save the barcode:

            try
            {
                result.Save(outputPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $" Unable to save the image: {ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private (string InputPath, string OutputPath, BarCodeParameters Parameters) GetValidatedParameters()
        {
            var inputPath = inputPathTextBox.Text;
            if (!File.Exists(inputPath))
            {
                throw new Exception("The input file does not exist.");
            }

            var outputPath = outputPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(outputPath) || outputPath.Any(x => Path.GetInvalidPathChars().Contains(x)))
            {
                throw new Exception("The output path is invalid.");
            }

            if (!Path.HasExtension(outputPath))
            {
                outputPath += ".png";
            }

            if (File.Exists(outputPath))
            {
                var promptResult = MessageBox.Show(
                    $"The file '{outputPath}' already exists. Are you sure you want to overwrite it?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (promptResult != DialogResult.Yes)
                {
                    throw new OperationCanceledException();
                }
            }

            if (!int.TryParse(barWidthTextBox.Text, out var barWidth))
            {
                throw new Exception("Invalid bar width.");
            }

            if (!int.TryParse(imageWidthTextBox.Text, out var imageWidth))
            {
                throw new Exception("Invalid output width.");
            }

            int? imageHeight = null;
            if (!useInputHeightForOutputCheckBox.Checked)
            {
                if (int.TryParse(imageHeightTextBox.Text, out var nonNullableImageHeight))
                {
                    imageHeight = nonNullableImageHeight;
                }
                else
                {
                    throw new Exception("Invalid output height.");
                }
            }

            var parameters = new BarCodeParameters()
            {
                Smoothen = smoothCheckBox.Checked,
                BarWidth = barWidth,
                Width = imageWidth,
                Height = imageHeight
            };

            return (inputPath, outputPath, parameters);
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
