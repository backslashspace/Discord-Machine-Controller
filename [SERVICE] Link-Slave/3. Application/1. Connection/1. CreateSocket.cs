using System.Net.Sockets;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static void CreateSocket()
        {
            socket = new(CurrentConfig.ServerIP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Blocking = true;
            socket.NoDelay = true;
            socket.ReceiveTimeout = 10240;
            socket.SendTimeout = 3840;
        }
    }
}