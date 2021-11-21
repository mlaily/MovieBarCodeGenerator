using MovieBarCodeGenerator.CLI;
using MovieBarCodeGenerator.GUI;
using System.Runtime.InteropServices;

namespace MovieBarCodeGenerator;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        if (!args.Any())
        {
            // GUI
            FreeConsole();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        else
        {
            // CLI
            new CLIBatchProcessor().Process(args);
        }
    }

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();
}
