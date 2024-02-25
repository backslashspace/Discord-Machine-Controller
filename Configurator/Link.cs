using Configurator.Pages;
using EXT.System.Service;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Configurator
{
    internal static class Link
    {
        internal static Task GenerateConfig()
        {
            AskUserForParams dialogue = new();
            dialogue.ShowDialog();

            if (dialogue.Result == null)
            {
                Dialogue msg = new("Error", "No data was returned, terminating..", Dialogue.Icons.Circle_Error, "OK");

                msg.ShowDialog();

                Environment.Exit(1);
            }

            try
            {
                Directory.CreateDirectory("C:\\Program Files\\Discord VMLink Slave");

                using (StreamWriter streamWriter = new($"C:\\Program Files\\Discord VMLink Slave\\config.txt", false, Encoding.UTF8))
                {
                    streamWriter.WriteLine(dialogue.Result[0]);
                }
            }
            catch (Exception ex)
            {
                Dialogue msg = new("Error", $"Unable to write config file to disk, terminating..\n\n{ex.Message}", Dialogue.Icons.Circle_Error, "OK");

                msg.ShowDialog();

                Environment.Exit(1);
            }

            ServerConfigDisplay.serverStringBox.Text = dialogue.Result[1];

            return Task.CompletedTask;
        }

        internal static async Task<String> InstallLink()
        {
            await GenerateConfig();

            String message = "";

            try
            {
                ExtractService();
                message += "extracted service to \"C:\\Program Files\\Discord VMLink Slave\\LinkSlave.exe\"\n";

                RegisterService();
                message += "registered service as 'Discord VMLink Slave'\n";

                await Task.Delay(1000);

                xService.SetStartupType("Discord VMLink Slave", ServiceStartMode.Automatic);
                message += "set start mode to automatic\n";
            }
            catch (Exception ex)
            {
                Dialogue msg = new("Error", $"Unable to extract file, terminating..\n\n{ex.Message}", Dialogue.Icons.Circle_Error, "OK");

                msg.ShowDialog();

                Environment.Exit(1);
            }

            return message;
        }

        //

        private static void ExtractService()
        {
            Directory.CreateDirectory("C:\\Program Files\\Discord VMLink Slave");

            using FileStream fileStream = File.Create("C:\\Program Files\\Discord VMLink Slave\\LinkSlave.exe");

            Assembly.GetExecutingAssembly().GetManifestResourceStream("Configurator.LinkSlave.exe").CopyTo(fileStream);
        }

        private static void RegisterService()
        {
            Process process = new();
            process.StartInfo.FileName = "C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe";
            process.StartInfo.Arguments = "-c \"New-Service -Name 'Discord VMLink Slave' -BinaryPathName 'C:\\Program Files\\Discord VMLink Slave\\LinkSlave.exe' -Description 'Discord VMLink endpoint service'";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }
    }
}