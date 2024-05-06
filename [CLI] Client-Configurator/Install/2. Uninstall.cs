using Microsoft.Win32;
using System;
using System.IO;

namespace Configurator
{
    internal static partial class Program
    {
        private static void UnInstall()
        {
            if (State.MMCServiceIsPresent)
            {
                RunSC($"delete \"{Config.ServiceName}\"");

                Console.WriteLine($" Queued removal of service:\n \"{Config.ServiceName}\"");
            }

            try
            {
                Directory.Delete(Config.InstallPath, true);
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
    }
}