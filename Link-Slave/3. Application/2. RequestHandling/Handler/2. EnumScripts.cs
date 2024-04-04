using System;
using System.IO;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static void EnumScripts(ref Byte errorCode)
        {
            String[] directories = Directory.GetFiles(CurrentConfig.ScriptDirectory);  // todo: test

            String formattedResult;

            Color responseColor;

            if (directories.Length == 0)
            {
                formattedResult = "*Script directory is empty*";
                responseColor = Color.Orange;
            }
            else
            {
                formattedResult = "**Content of Script directory**\n";
                responseColor = Color.Blue;

                for (Int32 i = 0; i < directories.Length; ++i)
                {
                    String[] pathParts = directories[i].Split('\\');

                    formattedResult += $"\n- {pathParts[pathParts.Length - 1]}";
                }
            }

            if (formattedResult.Length > 4090)
            {
                formattedResult = formattedResult.Substring(0, 4090);
                formattedResult += "...";
            }

            Byte[] rawResponse = ServerResponseBuilder(ref formattedResult, ref responseColor);

            if (AES_TCP.RefSend(ref errorCode, ref socket, ref rawResponse, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key) != 0)
            {
                return;
            }

            Log.FastLog("Main-Worker", "Successfully send script folder content to server", xLogSeverity.Info);
        }
    }
}