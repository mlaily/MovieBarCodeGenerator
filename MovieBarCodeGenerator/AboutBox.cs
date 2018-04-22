using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieBarCodeGenerator
{
    partial class AboutBox : Form
    {
        public AboutBox()
        {
            InitializeComponent();
            Text = "About";

            var assemblyInfo = Assembly.GetExecutingAssembly().GetName();

            titleLabel.Text = $"Movie Barcode Generator {assemblyInfo.Version}";

            textLabel.Text =
                $@"This program is open source, and released under the GPL license.
Copyright Melvyn Laïly.";

            linkLabel.Text = "https://zerowidthjoiner.net/movie-barcode-generator";
            linkLabel.LinkClicked += (s, e) => Process.Start(linkLabel.Text);
        }
    }
}
