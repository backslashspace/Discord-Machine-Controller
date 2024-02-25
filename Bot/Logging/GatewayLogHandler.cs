using Discord;
using System.Threading.Tasks;

namespace DC_SRV_VM_LINK.Bot
{
    internal static partial class SRV
    {
        private static async Task DCLogHandler(LogMessage formattedLogMessage)
        {
            await DistributeLog(formattedLogMessage);
        }
    }
}