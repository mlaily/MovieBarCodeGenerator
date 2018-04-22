using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieBarCodeGenerator
{
    public partial class MainForm : Form
    {
        private OpenFileDialog _openFileDialog;
        private SaveFileDialog _saveFileDialog;
        private FfmpegWrapper _ffmpegWrapper;

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

            _ffmpegWrapper = new FfmpegWrapper("fmpeg.exe");

            useInputHeightForOutputCheckBox.Checked = true;
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
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

        private void barCountTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (keepBarcodeValuesInSyncCheckBox.Checked)
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
        }

        private void barWidthTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (keepBarcodeValuesInSyncCheckBox.Checked)
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
        }

        private void aboutButton_Click(object sender, EventArgs e) => new AboutBox().ShowDialog();
    }
}
