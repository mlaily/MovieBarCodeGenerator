namespace MovieBarCodeGenerator.GUI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.inputPathTextBox = new System.Windows.Forms.TextBox();
            this.browseInputPathButton = new System.Windows.Forms.Button();
            this.generateButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.outputPathTextBox = new System.Windows.Forms.TextBox();
            this.browseOutputPathButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.smoothCheckBox = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.barWidthTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.barCountTextBox = new System.Windows.Forms.TextBox();
            this.useInputHeightForOutputCheckBox = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.imageHeightTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.imageWidthTextBox = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.aboutButton = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // inputPathTextBox
            // 
            this.inputPathTextBox.AllowDrop = true;
            this.inputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputPathTextBox.Location = new System.Drawing.Point(6, 37);
            this.inputPathTextBox.Name = "inputPathTextBox";
            this.inputPathTextBox.Size = new System.Drawing.Size(367, 20);
            this.inputPathTextBox.TabIndex = 1;
            this.inputPathTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_DragDrop);
            this.inputPathTextBox.DragOver += new System.Windows.Forms.DragEventHandler(this.TextBox_DragOver);
            // 
            // browseInputPathButton
            // 
            this.browseInputPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseInputPathButton.Location = new System.Drawing.Point(379, 35);
            this.browseInputPathButton.Name = "browseInputPathButton";
            this.browseInputPathButton.Size = new System.Drawing.Size(75, 23);
            this.browseInputPathButton.TabIndex = 2;
            this.browseInputPathButton.Text = "Browse...";
            this.browseInputPathButton.UseVisualStyleBackColor = true;
            this.browseInputPathButton.Click += new System.EventHandler(this.browseInputPathButton_Click);
            // 
            // generateButton
            // 
            this.generateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.generateButton.Location = new System.Drawing.Point(397, 240);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(75, 23);
            this.generateButton.TabIndex = 13;
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Input video path:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.outputPathTextBox);
            this.groupBox1.Controls.Add(this.browseOutputPathButton);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.inputPathTextBox);
            this.groupBox1.Controls.Add(this.browseInputPathButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(460, 117);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Files";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Output image path:";
            // 
            // outputPathTextBox
            // 
            this.outputPathTextBox.AllowDrop = true;
            this.outputPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputPathTextBox.Location = new System.Drawing.Point(6, 81);
            this.outputPathTextBox.Name = "outputPathTextBox";
            this.outputPathTextBox.Size = new System.Drawing.Size(367, 20);
            this.outputPathTextBox.TabIndex = 3;
            this.outputPathTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_DragDrop);
            this.outputPathTextBox.DragOver += new System.Windows.Forms.DragEventHandler(this.TextBox_DragOver);
            // 
            // browseOutputPathButton
            // 
            this.browseOutputPathButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseOutputPathButton.Location = new System.Drawing.Point(379, 79);
            this.browseOutputPathButton.Name = "browseOutputPathButton";
            this.browseOutputPathButton.Size = new System.Drawing.Size(75, 23);
            this.browseOutputPathButton.TabIndex = 4;
            this.browseOutputPathButton.Text = "Browse...";
            this.browseOutputPathButton.UseVisualStyleBackColor = true;
            this.browseOutputPathButton.Click += new System.EventHandler(this.browseOutputPathButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.smoothCheckBox);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.barWidthTextBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.barCountTextBox);
            this.groupBox2.Controls.Add(this.useInputHeightForOutputCheckBox);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.imageHeightTextBox);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.imageWidthTextBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 135);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(460, 99);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Barcode parameters";
            // 
            // smoothCheckBox
            // 
            this.smoothCheckBox.AutoSize = true;
            this.smoothCheckBox.Location = new System.Drawing.Point(349, 29);
            this.smoothCheckBox.Name = "smoothCheckBox";
            this.smoothCheckBox.Size = new System.Drawing.Size(106, 30);
            this.smoothCheckBox.TabIndex = 11;
            this.smoothCheckBox.Text = "Also generate\r\na smooth version";
            this.toolTip1.SetToolTip(this.smoothCheckBox, "A second image will be generated, with smoothed bars.\r\nThe file will be created n" +
        "ext to the specified output path, and will have the \"_smoothed\" suffix.");
            this.smoothCheckBox.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(190, 64);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(180, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "Note: bar width will take precedence";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(258, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Bar width:";
            // 
            // barWidthTextBox
            // 
            this.barWidthTextBox.Location = new System.Drawing.Point(258, 37);
            this.barWidthTextBox.Name = "barWidthTextBox";
            this.barWidthTextBox.Size = new System.Drawing.Size(55, 20);
            this.barWidthTextBox.TabIndex = 10;
            this.barWidthTextBox.Text = "1";
            this.barWidthTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.barWidthTextBox_KeyUp);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(190, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Bar count:";
            // 
            // barCountTextBox
            // 
            this.barCountTextBox.Location = new System.Drawing.Point(190, 37);
            this.barCountTextBox.Name = "barCountTextBox";
            this.barCountTextBox.Size = new System.Drawing.Size(55, 20);
            this.barCountTextBox.TabIndex = 9;
            this.barCountTextBox.Text = "1000";
            this.barCountTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.barCountTextBox_KeyUp);
            // 
            // useInputHeightForOutputCheckBox
            // 
            this.useInputHeightForOutputCheckBox.AutoSize = true;
            this.useInputHeightForOutputCheckBox.Location = new System.Drawing.Point(6, 63);
            this.useInputHeightForOutputCheckBox.Name = "useInputHeightForOutputCheckBox";
            this.useInputHeightForOutputCheckBox.Size = new System.Drawing.Size(172, 17);
            this.useInputHeightForOutputCheckBox.TabIndex = 8;
            this.useInputHeightForOutputCheckBox.Text = "Same height as the input video";
            this.useInputHeightForOutputCheckBox.UseVisualStyleBackColor = true;
            this.useInputHeightForOutputCheckBox.CheckedChanged += new System.EventHandler(this.useInputHeightForOutputCheckBox_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(64, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(12, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "x";
            // 
            // imageHeightTextBox
            // 
            this.imageHeightTextBox.Location = new System.Drawing.Point(79, 37);
            this.imageHeightTextBox.Name = "imageHeightTextBox";
            this.imageHeightTextBox.Size = new System.Drawing.Size(55, 20);
            this.imageHeightTextBox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(60, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Image size:";
            // 
            // imageWidthTextBox
            // 
            this.imageWidthTextBox.Location = new System.Drawing.Point(6, 37);
            this.imageWidthTextBox.Name = "imageWidthTextBox";
            this.imageWidthTextBox.Size = new System.Drawing.Size(55, 20);
            this.imageWidthTextBox.TabIndex = 6;
            this.imageWidthTextBox.Text = "1000";
            this.imageWidthTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.imageWidthTextBox_KeyUp);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(49, 240);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(342, 23);
            this.progressBar1.Step = 1;
            this.progressBar1.TabIndex = 6;
            // 
            // aboutButton
            // 
            this.aboutButton.Location = new System.Drawing.Point(12, 240);
            this.aboutButton.Name = "aboutButton";
            this.aboutButton.Size = new System.Drawing.Size(31, 23);
            this.aboutButton.TabIndex = 12;
            this.aboutButton.Text = "?";
            this.aboutButton.UseVisualStyleBackColor = true;
            this.aboutButton.Click += new System.EventHandler(this.aboutButton_Click);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 5000;
            this.toolTip1.InitialDelay = 100;
            this.toolTip1.ReshowDelay = 100;
            // 
            // logTextBox
            // 
            this.logTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logTextBox.Location = new System.Drawing.Point(12, 269);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.logTextBox.Size = new System.Drawing.Size(460, 80);
            this.logTextBox.TabIndex = 14;
            this.logTextBox.WordWrap = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.aboutButton);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.generateButton);
            this.MinimumSize = new System.Drawing.Size(500, 312);
            this.Name = "MainForm";
            this.Text = "Movie BarCode Generator";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox inputPathTextBox;
        private System.Windows.Forms.Button browseInputPathButton;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox outputPathTextBox;
        private System.Windows.Forms.Button browseOutputPathButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox barWidthTextBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox barCountTextBox;
        private System.Windows.Forms.CheckBox useInputHeightForOutputCheckBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox imageHeightTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox imageWidthTextBox;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button aboutButton;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.CheckBox smoothCheckBox;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox logTextBox;
    }
}

