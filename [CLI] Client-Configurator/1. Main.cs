using System;
using System.Diagnostics;
using System.Reflection;

namespace Configurator
{
    internal static partial class Program
    {
        private const String PackedVersion = "2.0 rc2";
        private static String InstallerFileVersionString;
        private static String AssemblyLocation;

        private static void Main()
        {
            PrepareWindow();

            GetState();

            ShowStateInfo();

            UInt32 userChoice = PrintOptions();

            Act(ref userChoice);
        }

        //

        private static UInt32 PrintOptions()
        {
            if (State.MMCServiceIsPresent)
            {
                Console.Write(" 1. Re-Install\n" +
                              " 2. Un-Install\n" +
                              " 3. Create new config\n" +
                              " 4. Exit\n\n" +
                              "Select> ");
            }
            else
            {
                Console.Write(" 1. Install\n\n" +
                              " 2. Exit\n\n" +
                              "Select> ");
            }

            String input = "";

            while (true)
            {
                try
                {
                    input = Console.ReadLine().Trim();

                    UInt32 choice = UInt32.Parse(input);

                    if (!State.ServiceIsPresent && choice < 3 && choice != 0)
                    {
                        return choice;
                    }
                    else if (State.ServiceIsPresent && choice < 5 && choice != 0)
                    {
                        return choice;
                    }

                    throw new Exception();
                }
                catch
                {
                    Console.CursorTop -= 1;

                    if (input == "")
                    {
                        Console.Write("\nSelect> ");
                    }
                    else
                    {
                        Console.CursorLeft = 8 + input.Length;

                        Console.Write(" - invalid input\nSelect> ");
                    }
                }
            }
        }

        private static void ShowStateInfo()
        {
            Console.WriteLine("███████████████████████████████████████████\n\n" +

                             $" Packed Version:                 [{PackedVersion}]\n\n" +

                             $" Directory is present:           {State.InstallDirectoryIsPresent}\n" +
                             $" Service executable is present:  {State.ServiceIsPresent}\n\n" +
                             
                             $" Service is registered:          {State.MMCServiceIsPresent}\n" +
                             $" Service is running:             {State.ServiceIsRunning}\n\n" +

                             $"▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄▄\n");
        }

        private static void PrepareWindow()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WindowWidth = 80;
            Console.WindowHeight = 20;

            AssemblyLocation = Assembly.GetExecutingAssembly().Location;

            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(AssemblyLocation);

            InstallerFileVersionString = $"{fileVersionInfo.FileMajorPart}.{fileVersionInfo.FileMinorPart}.{fileVersionInfo.FileBuildPart}.{fileVersionInfo.FilePrivatePart}";

            Console.Title = $"Discord-Machine-Endpoint Configurator v{InstallerFileVersionString}";
        }
    }
}