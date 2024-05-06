using System;
using System.Diagnostics;
using System.Threading;

namespace Configurator
{
    internal static partial class Program
    {
        private static void RegisterService()
        {
            Thread.Sleep(1024);

            Int32 exitCode = RunSC($"create \"{Config.ServiceName}\" type=own start=auto binpath=\"{Config.InstallPath}{Config.ServiceName}.exe\" displayname=\"{Config.ServiceName}\"");

            if (exitCode == 0)
            {
                goto FINISH;
            }

            while (exitCode == 1072)
            {
                Console.WriteLine($" Unable to register service:\n SC exit code was: {exitCode}\n Close all instances of mmc.exe\n Waiting 5120ms");

                Thread.Sleep(5120);

                exitCode = RunSC($"create \"{Config.ServiceName}\" type=own start=auto binpath=\"{Config.InstallPath}{Config.ServiceName}.exe\" displayname=\"{Config.ServiceName}\"");
            }

            if (exitCode != 0)
            {
                ErrorExit($" Failed to register service,\n exit code was: {exitCode}");
            }

            FINISH:

            Thread.Sleep(1024);

            RunSC($"description \"{Config.ServiceName}\" \"{Config.ServiceDescription}\"");

            Console.WriteLine(" Registered Service");
        }

        private static Int32 RunSC(String args)
        {
            Process process = new();
            process.StartInfo.FileName = "C:\\Windows\\System32\\sc.exe";
            process.StartInfo.Verb = "runas";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = args;
            process.Start();
            process.WaitForExit();

            return process.ExitCode;
        }
    }
}