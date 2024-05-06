using System;

namespace Configurator
{
    internal static partial class Program
    {
        private static void ErrorExit(String message)
        {
            Console.Write(message + "\n\nPress return to exit: ");

            Console.ReadLine();
            
            Environment.Exit(-1);
        }
    }
}