using Discord;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;
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

            Log.FastLog("Initiator", "Loading config", LogSeverity.Info);
            ConfigLoader.Load();

            Log.FastLog("Initiator", "Attempting to connect to discord", LogSeverity.Info);
            Bot.Connect();

            //## ## ## ## ## ## ## ## ## ## ## ##

            Thread test = new(() =>
            {
                while (true)
                {
                    Log.FastLog("Test", $"Test message: {GenText()}", LogSeverity.Debug);

                    Task.Delay(10000).Wait();
                }
            });
            test.Name = "test";
            test.Start();
        }

        //## ## ## ## ## ## ## ## ## ## ## ## ## ## ## ## ## ## ## ## ## ##

        private static String GenText()
        {
            IRandomizerString textGen = RandomizerFactory.GetRandomizer(new FieldOptionsTextLipsum());
            String output = textGen.Generate();

            return output.Split('.')[0] + '.';
        }
    }
}