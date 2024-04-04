using Org.BouncyCastle.Crypto.Fips;
using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Link_Master.Worker
{
    internal static partial class LinkFactory
    {
        private static void BindPort()
        {
            IPAddress ipAddress = CurrentConfig.TcpListenerIP;
            IPEndPoint localEndPoint = new(ipAddress, (Int32)CurrentConfig.TcpListenerPort);

            listener = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
            }
            catch
            {
                Log.FastLog("Link-Factory", $"Unable to bind port [{CurrentConfig.TcpListenerPort}] to [{CurrentConfig.TcpListenerIP}], terminating", xLogSeverity.Critical);

                Control.Shutdown.ServiceComponents();
            }

            Log.FastLog("Link-Factory", $"Successfully bound to [{CurrentConfig.TcpListenerIP}:{CurrentConfig.TcpListenerPort}], entering listening state", xLogSeverity.Info);
        }

        private static Boolean InterruptibleAccept()
        {
            Thread accepter = new(() =>
            {
                try
                {
                    socket = listener.Accept();

                    socket.Blocking = true;
                    socket.NoDelay = true;
                    socket.ReceiveTimeout = 5120;
                    socket.SendTimeout = 5120;
                }
                catch { }
            });

            accepter.Name = "New link connection accepter";
            accepter.Start();

            while (!WorkerThreads.LinkFactory_WasCanceled && accepter.IsAlive)
            {
                Task.Delay(256).Wait();
            }

            if (WorkerThreads.LinkFactory_WasCanceled)
            {
                listener.Close(0);

                for (Byte b = 0; b < 16 && accepter.ThreadState != ThreadState.Stopped; ++b)
                {
                    Task.Delay(64).Wait();
                }

                return true;
            }

            Log.FastLog("Link-Factory", $"{(socket.RemoteEndPoint as IPEndPoint).Address} established a connection", xLogSeverity.Info);

            return false;
        }

        private static void CloseConnection(ref FipsSecureRandom random)
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

            Task.Delay(random.Next(5120)).Wait();
        }
    }
}