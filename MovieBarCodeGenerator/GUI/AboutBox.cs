using System.Diagnostics;
using System.Reflection;

namespace MovieBarCodeGenerator.GUI;

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
