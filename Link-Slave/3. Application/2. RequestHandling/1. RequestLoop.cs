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

        private static Byte RequestHandlerLoop()
        {
            Byte[] buffer;
            Byte errorCode = 0;
            
            while (!WorkerThread.Worker_WasCanceled)
            {
                try
                {
                    buffer = AES_TCP.Receive(ref socket, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
                }
                catch { return 1; }

                if ((RequestTypes)buffer[0] == RequestTypes.UAliveQuestionMark)
                {
                    KeepAlive(ref buffer, ref errorCode);
                }
                else
                {
                    Log.FastLog("Main-Worker", $"Received Request: {(RequestTypes)buffer[0]}", xLogSeverity.Info);

                    switch ((RequestTypes)buffer[0])
                    {
                        case RequestTypes.EnumScripts:
                            EnumScripts(ref errorCode);
                            break;

                        case RequestTypes.ExecuteScript:
                            ExecuteScript(ref errorCode, ref buffer);
                            break;

                        case RequestTypes.RemoteDownload:
                            break;

                        default:
                            NotImplemented(ref buffer[0], ref errorCode);
                            break;
                    }
                }

                if (errorCode != 0)
                {
                    return errorCode;
                }
            }

            return 0;
        }
    }
}