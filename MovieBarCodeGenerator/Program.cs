using MovieBarCodeGenerator.CLI;
using MovieBarCodeGenerator.GUI;
using Nito.AsyncEx;
using System.Runtime.InteropServices;

namespace MovieBarCodeGenerator;

static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread] // Note: this attribute is ignored on async Main() entry points...
    static void Main(string[] args)
    {
        if (args.Any())
        {
            // CLI
            AsyncContext.Run(async () =>
            {
                await new CLIBatchProcessor().ProcessAsync(args);
            });
        }
        else
        {
            // GUI
            FreeConsole();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();
}
