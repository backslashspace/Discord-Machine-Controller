using System.Net.Sockets;
using System.Net;
//
using static Link_Master.Worker.Bot;

namespace Link_Master.Worker
{
    internal static partial class LinkWorker
    {
        private static void DeregisterLink(ref Socket socket, ref ChannelLink channelLink)
        {
            if (ActiveMachineLinks == null)
            {
                Log.FastLog("Machine-Link", $"ConcurrentDictionary 'ActiveMachineLinks' was 'null', this should not happen, terminating service", xLogSeverity.Critical);

                Control.Shutdown.ServiceComponents();
            }

            if (!ActiveMachineLinks.TryRemove(channelLink.ChannelID, out _))
            {
                Log.FastLog("Machine-Link", $"Unable to remove endpoint '{channelLink.Name}' ({(socket.RemoteEndPoint as IPEndPoint).Address}) from ConcurrentDictionary (not in dictionary)", xLogSeverity.Error);
            }
        }

        private static void Disconnect(ref Socket socket)
        {
            try
            {
                socket.Disconnect(false);
            }
            catch { }

            try
            {
                socket.Close(0);
            }
            catch { }
        }
    }
}