using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Link_Master.Control
{
    internal static partial class Start
    {
        internal static async void Initiate()
        {
            StartLogWorker();

            Log.FastLog("Initiator", "Loading config", xLogSeverity.Info);
            ConfigLoader.Load();
            Log.FastLog("Initiator", "Config loaded", xLogSeverity.Info);

            Log.FastLog("Initiator", "Starting machine link factory", xLogSeverity.Info);
            WorkerThreads.LinkFactory = new(() => Worker.LinkFactory.Worker());
            WorkerThreads.LinkFactory.Name = "Link Factory";
            WorkerThreads.LinkFactory.Start();

            Log.FastLog("Initiator", "Attempting to connect to discord", xLogSeverity.Info);
            await Worker.Bot.Connect();









            //## ## ## ## ## ## ## ## ## ## ## ##

            await Task.Delay(4000);

            Thread test3 = new(() =>
            {
                while (true)
                {
                    Log.FastLog("Test", $"Test message: {GenText()}", xLogSeverity.Debug);

                    Task.Delay(10000).Wait();
                }
            });
            test3.Name = "test";
            //test3.Start();
        }

        private static String GenText()
        {
            IRandomizerString textGen = RandomizerFactory.GetRandomizer(new FieldOptionsTextLipsum());
            String output = textGen.Generate();

            return output.Split('.')[0] + '.';
        }
    }
}