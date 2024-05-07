using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace Configurator
{
    internal static partial class Program
    {
        private static void UnInstall()
        {
            StopService();

            if (State.MMCServiceIsPresent)
            {
                RunSC($"delete \"{Config.ServiceName}\"");

                Console.WriteLine($" Queued removal of service: \"{Config.ServiceName}\"");
            }

            try
            {
                String[] files = Directory.GetFiles(Config.InstallPath);

                for (Int32 i = 0; i < files.Length; ++i)
                {
                    try
                    {
                        File.Delete(files[i]);
                    }
                    catch { }
                }
            }
            catch { }

            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey($"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);

                if (key.OpenSubKey(Config.ServiceName, true) != null)
                {
                    key.DeleteSubKeyTree(Config.ServiceName);

                    Console.WriteLine(" Deregistered Windows App");
                }
            }
            catch (Exception ex)
            {
                ErrorExit(" Failed to check / remove item from registry: " + ex.Message);
            }
        }

        private static void StopService()
        {
            Process process = new();
            process.StartInfo.FileName = "C:\\Windows\\System32\\net.exe";
            process.StartInfo.Verb = "runas";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = $"stop \"{Config.ServiceName}\"";
            process.Start();
            process.WaitForExit();
        }
    }
}