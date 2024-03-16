using Discord;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class Log
    {
        internal static ConcurrentQueue<InternalLogMessage> logQueue = new();

        internal static CancellationTokenSource tokenSource = new();

        internal static void Worker(CancellationToken cancellationToken)
        {
            //empty line in log file for better readability
            using (StreamWriter streamWriter = new($"{Program.assemblyPath}\\logs\\{DateTime.Now:dd.MM.yyyy}.txt", true, Encoding.UTF8))
            {
                streamWriter.WriteLine();
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                while (logQueue.IsEmpty && !cancellationToken.IsCancellationRequested)
                {
                    Task.Delay(256).Wait();
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    continue;
                }

                //

                logQueue.TryDequeue(out InternalLogMessage internalLogMessage);

                //

                File(ref internalLogMessage);

                if (!IgnoreNew)
                {
                    Console(ref internalLogMessage);
                }

                Discord(ref internalLogMessage);
            }
        }

        private static void File(ref InternalLogMessage internalLogMessage)
        {
            try
            {
                if (!internalLogMessage.BypassFile)
                {
                    PushFile(ref internalLogMessage.LogMessage, ref internalLogMessage.TimeStamp);
                }
            }
            catch (Exception ex)
            {
                FastLog("Log-Worker", $"An error occurred while writing a log message to the log file, message was: {ex.Message}", LogSeverity.Error, bypassFile: true);
            }
        }

        private static void Console(ref InternalLogMessage internalLogMessage)
        {
            try
            {
                EnqueueConsole(ref internalLogMessage.LogMessage, ref internalLogMessage.TimeStamp, ref internalLogMessage.BypassIPC);
            }
            catch (Exception ex)
            {
                FastLog("Log-Worker", $"An error occurred while enqueueing a log message to the log console worker, message was: {ex.Message}", LogSeverity.Error, bypassConsole: true);
            }
        }

        private static void Discord(ref InternalLogMessage internalLogMessage)
        {
            try
            {
                if (CurrentConfig.logChannel != null && Client.IsConnected && !internalLogMessage.BypassDiscord)
                {
                    PushDiscord(ref internalLogMessage.LogMessage, ref internalLogMessage.TimeStamp);
                }
            }
            catch (Exception ex)
            {
                FastLog("Log-Worker", $"An error occurred while sending a log message to Discord, message was: {ex.Message}", LogSeverity.Error, bypassDiscord: true);
            }
        }
    }
}