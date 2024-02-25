using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Service
{
    public partial class Service : ServiceBase
    {
        internal Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(String[] args)
        {
            StartStop.Start(args);
        }

        protected override void OnStop()
        {
            StartStop.PrepShutdown();
        }
    }
}