using System.Net.Sockets;

namespace Link_Master.Worker
{
    internal static partial class LinkWorker
    {
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