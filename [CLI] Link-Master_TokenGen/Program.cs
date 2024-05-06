using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace TokenGen
{
    internal class Program
    {
        private static String path;

        private static String Titel;

        static void Main()
        {
            Console.CancelKeyPress += ABORT;

            PrepareWindowPlusEnv();

            Console.Write("Trailing whitespaces will be removed.\nGenerator compatibility level: 1\n\nEnter token: ");
            Int32 line = Console.CursorTop;
            String rawInput = Console.ReadLine();
            String input = rawInput.Trim();

            String padding = "";

            for (UInt16 i = 0; i < 4 + rawInput.Length - input.Length; ++i)
            {
                padding += " ";
            }

            Console.SetCursorPosition(0, line);
            Console.WriteLine($"Using: \"{input}\"{padding}\n");

            WriteEncodedTokenToDisk(ref input);

            Console.Write("\nPress return to exit: ");
            Console.ReadLine();

            Console.Title = Titel;
        }

        //

        private static void ABORT(object sender, ConsoleCancelEventArgs e)
        {
            Console.Title = Titel;
        }

        private static void PrepareWindowPlusEnv()
        {
            Titel = Console.Title;
            Console.Title = "Master token generator v" + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        private static void WriteEncodedTokenToDisk(ref String input)
        {
            try
            {
                Byte[] rawToken = Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes(input)));

                using (FileStream fileStream = new(path + "\\token", FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    fileStream.Write(rawToken, 0, rawToken.Length);
                }

                Console.WriteLine($"Successfully wrote token to: \"{path}\\token\"");
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Unable to write data do disk, error was: " + ex.Message);

                Environment.ExitCode = 1;
            }
        }
    }
}