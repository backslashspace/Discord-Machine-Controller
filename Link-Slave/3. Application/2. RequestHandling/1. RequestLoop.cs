using System;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        internal enum RequestTypes : Byte
        {
            UAliveQuestionMark = 0x01,
            YuesAmIAlive = 0x02,

            EnumScripts = 0x03,
            ExecuteScript = 0x04,
            RemoteDownload = 0x05
        }

        private static void RequestHandlerLoop()
        {
            Log.FastLog("Main-Worker", $"Successfully connected and authenticated on [{CurrentConfig.ServerIP}:{CurrentConfig.TcpPort}], ready to process requests", xLogSeverity.Info);

            Byte[] buffer;

            while (!WorkerThread.Worker_WasCanceled)
            {
                buffer = AES_TCP.Receive(ref socket, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);

                switch ((RequestTypes)buffer[0])
                {
                    case RequestTypes.UAliveQuestionMark:
                        KeepAlive(ref buffer);
                        break;

                    case RequestTypes.EnumScripts:
                        EnumScripts();
                        break;

                    case RequestTypes.ExecuteScript:
                        break;

                    case RequestTypes.RemoteDownload:
                        break;

                    default:
                        NotImplemented(ref buffer[0]);
                        break;
                }
            }
        }
    }
}