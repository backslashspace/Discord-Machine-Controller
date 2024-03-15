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

        internal static CancellationTokenSource cancellationTokenSource = new();

        internal static void Worker(CancellationToken cancellationToken)
        {
            FastLog("Initiator", "Started log worker", LogSeverity.Info);

            while (!cancellationToken.IsCancellationRequested)
            {
                while (logQueue.IsEmpty)
                {
                    Task.Delay(256).Wait();
                }

                logQueue.TryDequeue(out InternalLogMessage internalLogMessage);

                if (!internalLogMessage.BypassFile)
                {
                    PushFile(ref internalLogMessage.LogMessage, ref internalLogMessage.TimeStamp);
                }

                EnqueueIPC(ref internalLogMessage.LogMessage, ref internalLogMessage.TimeStamp, ref internalLogMessage.BypassIPC);

                if (CurrentConfig.logChannel != null && Client.IsConnected && !internalLogMessage.BypassDiscord)
                {
                    PushDiscord(ref internalLogMessage.LogMessage, ref internalLogMessage.TimeStamp);
                }
            }

            //

            LogConsole.tokenSource.Cancel();

            while (WorkerThreads.LocalConsoleLogWorker.IsAlive)
            {
                Task.Delay(256).Wait();
            }

            using StreamWriter streamWriter = new($"{Program.assemblyPath}\\logs\\{DateTime.Now:dd.MM.yyyy}.txt", true, Encoding.UTF8);
            streamWriter.WriteLine();
        }
    }
}