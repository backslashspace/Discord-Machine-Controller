using System;
using System.ServiceProcess;
using System.Threading;
using VMLink_Slave;

namespace LinkSlave
{
    public partial class Service : ServiceBase
    {
        private static Thread worker;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(String[] args)
        {
            Config.Load();

            worker = new(() => Client.ConnectionLoop(token))
            {
                IsBackground = true,
                Name = "Main Worker"
            };
            worker.Start();
        }

        internal static CancellationTokenSource cancellationSource = new();
        internal static CancellationToken token = cancellationSource.Token;

        protected override void OnStop()
        {
            cancellationSource.Cancel();

            Log.Print("[Win32] - requested service shutdown", LogSeverity.Info);

            worker.Join();

            Log.Print("Shutdown completed", LogSeverity.Info);

            Client.Exit(true);  
        }
    }
}