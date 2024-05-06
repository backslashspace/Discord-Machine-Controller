using System;
using System.Runtime.InteropServices;

namespace Configurator
{
    internal class CustomAppEntry
    {
        //'overrides' Main() in App.g.cs
        [STAThread()]
        public static void Main()
        {
            try
            {
                App app = new();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                AllocConsole();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("Error message: ");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(ex.Message);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("\nStackTrace: ");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(ex.StackTrace);

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("\nAn unknown error occurred in the application.");

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("\nPress return to exit: ");

                Console.ReadLine();

                Environment.Exit(1);
            }
        }

        [DllImport("Kernel32")]
        internal static extern Boolean AllocConsole();
    }
}