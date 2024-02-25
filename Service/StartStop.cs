using System;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Service
{
    internal static class StartStop
    {
        internal static void Start(String[] args)
        {
            Bot.SRV.SRV_Main();
        }

        internal static void PrepShutdown()
        {
            Bot.Exit.Service(false);
        }
    }
}