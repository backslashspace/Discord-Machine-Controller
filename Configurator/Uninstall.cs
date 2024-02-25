using EXT.System.Service;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Configurator
{
    internal static class Uninstall
    {
        internal static async Task<Boolean> Commit()
        {
            Boolean result = false;

            try
            {
                try
                {
                    xService.Stop("Discord VMLink Slave");

                    await Task.Delay(3840);
                }
                catch { }
                
                Process process = new();
                process.StartInfo.FileName = "C:\\Windows\\System32\\sc.exe";
                process.StartInfo.Arguments = "delete \"Discord VMLink Slave\"";
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();


                try
                {
                    File.Delete("C:\\Program Files\\Discord VMLink Slave\\LinkSlave.exe");

                    File.Delete("C:\\Program Files\\Discord VMLink Slave\\config.txt");
                }
                catch { }

                result = true;
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}