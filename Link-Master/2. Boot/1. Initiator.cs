using Discord;
using Pcg;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Link_Master.Worker.Control
{
    internal static partial class Boot
    {
        internal static void Initiate()
        {
            WorkerThreads.LogWorker = new(() => Log.Worker(Log.tokenSource.Token))
            {
                Name = "Log Worker",
            };
            WorkerThreads.LogWorker.Start();
            Log.FastLog("Initiator", "Started log worker", LogSeverity.Info);

            WorkerThreads.LocalConsoleLogWorker = new(() => LogConsole.ConsoleServer(LogConsole.tokenSource.Token))
            {
                Name = "Local Log TCP Server",
            };
            WorkerThreads.LocalConsoleLogWorker.Start();
            Log.FastLog("Initiator", $"Started local console log server", LogSeverity.Info);

            ConfigLoader.Load();

            //## ## ## ## ## ## ## ## ## ## ## ##

            PcgRandom random = new();
            Byte[] buffer = new Byte[8];

            Thread test = new(() =>
            {
                random.NextBytes(buffer);

                Log.FastLog("Test", $"Test message: {Encoding.UTF8.GetString(buffer)}", LogSeverity.Debug);

                Task.Delay(2000).Wait();
            });
            test.Name = "test";
            test.Start();

            //connect

            //ConfigLoader.PostLoad();
        }
    }
}