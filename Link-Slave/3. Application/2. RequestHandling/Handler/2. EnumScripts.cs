using System;
using System.IO;
using System.Threading;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static void EnumScripts(ref Byte errorCode)
        {
            Thread.Sleep(10000);

            String[] directories;
            Byte[] rawResponse;

            String formattedResult;
            Color responseColor;

            try
            {
                directories = Directory.GetFiles(CurrentConfig.ScriptDirectory);
            }
            catch (Exception ex)
            {
                if (errorCounter == MaxErrorCount - 1)
                {
                    formattedResult = $"Failed to enumerate files in configured script directory, error was: {ex.Message}\n\nMaximum amount of errors reached ({MaxErrorCount + 1}), shutting down services";
                }
                else
                {
                    formattedResult = $"Failed to enumerate files in configured script directory, error was: {ex.Message}";
                }

                responseColor = Color.Red;

                rawResponse = ServerResponseBuilder(ref formattedResult, ref responseColor);

                Log.FastLog("Main-Worker", formattedResult, xLogSeverity.Error);
                AES_TCP.RefSend(ref errorCode, ref socket, ref rawResponse, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
                Log.FastLog("Main-Worker", "Send error response", xLogSeverity.Info);

                if (errorCounter == MaxErrorCount - 1)
                {
                    ErrorExit();
                }

                throw;
            }

            //

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

            rawResponse = ServerResponseBuilder(ref formattedResult, ref responseColor);

            if (AES_TCP.RefSend(ref errorCode, ref socket, ref rawResponse, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key) != 0)
            {
                return;
            }

            Log.FastLog("Main-Worker", "Successfully send script folder content to server", xLogSeverity.Info);
        }
    }
}