namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static void DisposeSocket()
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