using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Configurator
{
    internal static partial class Program
    {
        private static void GetRepoPath()
        {
            Console.Write($" Use default Script-Directory?\n\n {Config.DefaultScriptRepo}");

            if (AskYesNo())
            {
                ClearUserSelectionArea(13);

                Console.WriteLine(" - Using default path: '" + Config.DefaultScriptRepo + "'\n");

                Directory.CreateDirectory(Config.DefaultScriptRepo);
            }
            else
            {
                Console.Write("\n Enter Path without parentheses");

                State.RepoPath = _Path();

                ClearUserSelectionArea(13);

                Console.WriteLine(" - Using repo path: '" + State.RepoPath + "'\n");
            }
        }

        private static String _Path()
        {
            Console.Write("\n\nPath> ");

            while (true)
            {
                String input = Console.ReadLine().Trim();

                try
                {
                    if (Regex.IsMatch(input, ".:\\\\[^\\\\]"))
                    {
                        Directory.CreateDirectory(input);

                        if (Directory.Exists(input))
                        {
                            return input;
                        }
                    }
                }
                catch { }

                Err(input.Length == 0 ? -1 : input.Length);
            }

            static void Err(Int32 inputLength)
            {
                Console.CursorTop -= 1;

                Console.CursorLeft = 6 + (inputLength == 0 ? -1 : inputLength);

                Console.Write($" - invalid input\nPath> ");
            }
        }
    }
}