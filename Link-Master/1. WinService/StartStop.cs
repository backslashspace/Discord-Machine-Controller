﻿using Discord;
using System;
using System.IO;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Link_Master
{
    internal partial class Program
    {

        internal static void Start(String[] args)
        {
            //Task.Delay(10000).Wait();

            using (StreamWriter streamWriter = new($"{Program.assemblyPath}\\logs\\{DateTime.Now:dd.MM.yyyy}.txt", true, Encoding.UTF8))
            {
                streamWriter.WriteLine();   //create new line in log for better readability
            }

            Log.FastLog("Win32", $"Service [v{version}] start initiated", LogSeverity.Info);

            Control.Boot.Initiate();
        }

        internal static void Stop()
        {
            Control.Shutdown.ServiceComponents(false);
        }
    }
}