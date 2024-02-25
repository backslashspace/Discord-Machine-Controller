using Discord;
using System;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        internal static async Task FastLog(String category, String message, LogSeverity logSeverity, Boolean bypassClientQueue = false)
        {
            await DistributeLog(new LogMessage(logSeverity, category, message), bypassClientQueue);
        }
    }
}