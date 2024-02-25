using LinkSlave.Win;
using System;
using System.Threading;
using VMLink_Slave;

namespace LinkSlave
{
    internal static partial class Client
    {
        private static void PassivePingPong(ref CancellationToken cancellation)
        {
            Log.Print("Entered listening state", LogSeverity.Info);

            Byte[] requestBuffer = new Byte[1];

            while (true && !cancellation.IsCancellationRequested)
            {
                socket.ReceiveTimeout = -1;
                requestBuffer = AES_FastSocket.ReceiveTCP(ref socket, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
                socket.ReceiveTimeout = 3840;

                switch (requestBuffer[0])
                {
                    case (Byte)MastersRequests.UAliveQuestionMark:
                        AES_FastSocket.SendTCP(ref socket, new byte[] { (Byte)MastersRequests.YuesAmIAlive }, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
                        break;

                    case (Byte)MastersRequests.EnumScripts:
                        Log.Print("Script folder enumeration requested", LogSeverity.Info);
                        EnumScripts();
                        break;

                    case (Byte)MastersRequests.ExecuteScript:
                        Log.Print("Script execution requested", LogSeverity.Info);
                        ExecuteScript();
                        break;

                    case (Byte)MastersRequests.RemoteDownload:
                        Log.Print("File upload requested", LogSeverity.Info);
                        RemoteDownload();
                        break;

                    default:
                        Log.Print("Server send unknown request, ask your administrator to update your client", LogSeverity.Warning);
                        NotImplemented();
                        break;
                }
            }
        }

        private static void NotImplemented()
        {
            String responseMessage = $"Client: Unknown server request, client not up to date?\n\nClient version is v{Program.Version}";

            Color responseColor = Color.Purple;

            AES_FastSocket.SendTCP(ref socket, ServerResponseBuilder(ref responseMessage, ref responseColor), CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
        }
    }

    internal enum MastersRequests : Byte
    {
        UAliveQuestionMark = 0x01,
        YuesAmIAlive = 0x02,

        EnumScripts = 0x03,
        ExecuteScript = 0x04,
        RemoteDownload = 0x05
    }
}