using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Configurator
{
    internal static partial class Program
    {
        private static void OutPut()
        {
            String basedKeys = Convert.ToBase64String(State.Keys);

            WriteClientConfigToDisk(ref basedKeys);

            StartService();

            Console.WriteLine("\nConfig String:\n\n" +
                              $"vmChannelLink: \"{State.Name}:{State.Guid}:{State.ScriptTimeout}:{State.ChannelID}:{basedKeys}\"\n" +
                              
                              "\nPress return to exit: ");

            Console.ReadLine();
            Environment.Exit(0);
        }

        private static void StartService()
        {
            Process process = new();
            process.StartInfo.FileName = "C:\\Windows\\System32\\net.exe";
            process.StartInfo.Verb = "runas";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.Arguments = $"start \"{Config.ServiceName}\"";
            process.Start();
            process.WaitForExit();
        }

        private static void WriteClientConfigToDisk(ref readonly String basedKeys)
        {
            using (StreamWriter streamWriter = new($"{Config.InstallPath}config.txt", true, Encoding.UTF8))
            {
                streamWriter.Write($"scriptDirectory: \"{State.RepoPath}\"\n" +
                                   $"serverPort: \"{State.ServerPort}\"\n" +
                                   $"serverIP: \"{State.ServerIP}\"\n" +
                                   $"channelID: \"{State.ChannelID}\"\n" +
                                   $"guid: \"{State.Guid}\"\n" +
                                   $"name: \"{State.Name}\"\n" +
                                   $"keys: \"{basedKeys}\"");
            }
        }
    }
}