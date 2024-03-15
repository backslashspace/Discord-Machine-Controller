using System;

namespace Link_Master
{
    internal partial class Program
    {

        internal static void Start(String[] args)
        {
            Worker.Control.Boot.Initiate();
        }

        internal static void Stop()
        {
            Worker.Control.Shutdown.ServiceComponents(false);
        }
    }
}