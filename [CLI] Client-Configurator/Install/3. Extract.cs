using Microsoft.Win32;
using System;
using System.IO;

namespace Configurator
{
    internal static partial class Program
    {
        private static void Extract()
        {
            try
            {
                Directory.CreateDirectory(Config.InstallPath);

                using (FileStream bcDLL = File.Create($"{Config.InstallPath}bc-fips-1.0.2.dll"))
                {
                    State.Assembly.GetManifestResourceStream("Configurator.resources.bc-fips-1.0.2.dll").CopyTo(bcDLL);
                }

                using (FileStream bssDLL = File.Create($"{Config.InstallPath}BSS.Encryption.dll"))
                {
                    State.Assembly.GetManifestResourceStream("Configurator.resources.BSS.Encryption.dll").CopyTo(bssDLL);
                }

                using (FileStream bssDLL = File.Create($"{Config.InstallPath}System.Management.Automation.dll"))
                {
                    State.Assembly.GetManifestResourceStream("Configurator.resources.System.Management.Automation.dll").CopyTo(bssDLL);
                }

                using (FileStream exeFile = File.Create($"{Config.InstallPath}Discord Link-Slave.exe"))
                {
                    State.Assembly.GetManifestResourceStream("Configurator.resources.Discord Link-Slave.exe").CopyTo(exeFile);
                }

                using (FileStream configFile = File.Create($"{Config.InstallPath}Discord Link-Slave.exe.config"))
                {
                    State.Assembly.GetManifestResourceStream("Configurator.resources.Discord Link-Slave.exe.config").CopyTo(configFile);
                }

                File.Copy(AssemblyLocation, Config.InstallPath + Path.GetFileName(AssemblyLocation), true);

                Console.WriteLine(" Extracted Files");

                RegisterWindowAsApp();
            }
            catch (Exception ex)
            {
                ErrorExit(" Failed to extract files: " + ex.Message);
            }
        }

        //

        private static void RegisterWindowAsApp()
        {
            UInt32 estimatedSize = 0;

            try
            {
                DirectoryInfo directoryInfo = new(Config.InstallPath);

                UInt64 size = 0;

                FileInfo[] fileInfos = directoryInfo.GetFiles();

                for (Int32 i = 0; i < fileInfos.Length; ++i)
                {
                    size += (UInt64)fileInfos[i].Length;
                }

                estimatedSize = unchecked((UInt32)(size / 1024));
            }
            catch (Exception ex)
            {
                ErrorExit($" Unable to calculate size of install directory:\n {ex.Message}");
            }

            String REGPATH = $"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{Config.ServiceName}";

            String displayIcon = $"{Config.InstallPath}Client-Configurator.exe";
            String displayName = "Discord Remote Control Client";
            String displayVersion = $"{InstallerFileVersionString}";
            String publisher = "https://github.com/backslashspace";
            String uninstallString = $"{Config.InstallPath}Client-Configurator.exe";

            //

            try
            {
                Registry.SetValue(REGPATH, "DisplayIcon", displayIcon, RegistryValueKind.String);
                Registry.SetValue(REGPATH, "DisplayName", displayName, RegistryValueKind.String);
                Registry.SetValue(REGPATH, "DisplayVersion", displayVersion, RegistryValueKind.String);
                Registry.SetValue(REGPATH, "EstimatedSize", unchecked((Int32)estimatedSize), RegistryValueKind.DWord);
                Registry.SetValue(REGPATH, "NoModify", 1, RegistryValueKind.DWord);
                Registry.SetValue(REGPATH, "NoRepair", 1, RegistryValueKind.DWord);
                Registry.SetValue(REGPATH, "Publisher", publisher, RegistryValueKind.String);
                Registry.SetValue(REGPATH, "UninstallString", uninstallString, RegistryValueKind.String);
            }
            catch (Exception ex)
            {
                ErrorExit($" Unable to set registry values:\n {ex.Message}");
            }

            Console.WriteLine(" Registered as Windows App");
        }
    }
}