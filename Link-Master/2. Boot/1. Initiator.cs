using Discord;
using RandomDataGenerator.FieldOptions;
using RandomDataGenerator.Randomizers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Link_Master.Control
{
    internal static partial class Boot
    {
        internal static async void Initiate()
        {
            StartLogWorker();

            Log.FastLog("Initiator", "Loading config", LogSeverity.Info);
            ConfigLoader.Load();

            //Log.FastLog("Initiator", "Starting machine link manager", LogSeverity.Info);
            //ff

            Log.FastLog("Initiator", "Attempting to connect to discord", LogSeverity.Info);
            await Worker.Bot.Connect();

            //## ## ## ## ## ## ## ## ## ## ## ##

            await Task.Delay(4000);

            Thread test3 = new(() =>
            {
                while (true)
                {
                    Log.FastLog("Test", $"Test message: {GenText()}", LogSeverity.Debug);

                    Task.Delay(10000).Wait();
                }
            });
            test3.Name = "test";
            test3.Start();
        }

        private static String GenText()
        {
            IRandomizerString textGen = RandomizerFactory.GetRandomizer(new FieldOptionsTextLipsum());
            String output = textGen.Generate();

            return output.Split('.')[0] + '.';
        }
    }
}