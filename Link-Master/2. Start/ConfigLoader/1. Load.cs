using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Link_Master.Control
{
    internal static partial class ConfigLoader
    {
        internal static void Load()
        {
            List<String> configLines = new();

            ReadConfigFile(ref configLines);

            if (configLines.Count > 128)
            {
                Log.FastLog("Initiator", "Consider storing config non-mandatory data in a different file, terminating", xLogSeverity.Error);

                Shutdown.ServiceComponents();
            }

            try
            {
                ParseSettings(ref configLines);

                VerifyMandatorySettings();
            }
            catch (Exception ex)
            {
                Log.FastLog("Initiator", $"Unknow error while loading config, terminating", xLogSeverity.Critical);
                Log.FastLog("Initiator", $"Message: {ex.Message}\n\nSource: {ex.Source}\n\nStackTrace: {ex.StackTrace}", xLogSeverity.Verbose);

                Shutdown.ServiceComponents();
            }
        }

        //

        private static void ReadConfigFile(ref List<String> configLines)
        {
            try
            {
                if (!File.Exists($"{Program.assemblyPath}\\config.txt"))
                {
                    CreateConfigTemplate();

                    Log.FastLog("Initiator", "Missing config file, created a template in assembly file location, terminating", xLogSeverity.Verbose);

                    Shutdown.ServiceComponents();
                }

                using StreamReader streamReader = new($"{Program.assemblyPath}\\config.txt", Encoding.UTF8);

                String item;

                while ((item = streamReader.ReadLine()) != null)
                {
                    configLines.Add(item);
                }
            }
            catch (Exception ex)
            {
                Log.FastLog("Initiator", "Failed to read config file, terminating", xLogSeverity.Critical);
                Log.FastLog("Initiator", ex.Message, xLogSeverity.Verbose);

                Shutdown.ServiceComponents();
            }
        }

        //

        private static void Error(String msg)
        {
            Log.FastLog("Initiator", msg, xLogSeverity.Critical);

            Shutdown.ServiceComponents();
        }
    }
}