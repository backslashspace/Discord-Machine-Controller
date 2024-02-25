using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using VMLink_Slave;

namespace LinkSlave
{
    internal static partial class Client
    {
        static readonly HttpClient http_client = new();

        //

        private static void EnumScripts()
        {
            String[] directories = Directory.EnumerateFiles(CurrentConfig.scriptDirectory).ToArray();

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

            AES_FastSocket.SendTCP(ref socket, ServerResponseBuilder(ref formattedResult, ref responseColor), CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);

            Log.Print("Successfully send script folder content to server", LogSeverity.Info);
        }

        private static void ExecuteScript()
        {
            Byte[] rawFileName = AES_FastSocket.ReceiveTCP(ref socket, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);

            String fileName = Encoding.UTF8.GetString(rawFileName).Trim();

            String[] directories = Directory.EnumerateFiles(CurrentConfig.scriptDirectory).ToArray();

            Boolean repoContainsFile = false;
            String[] fullPathParts = null;
            String fullPath = null;

            Color responseColor = Color.Red;
            String responseMessage = "";
            Boolean wasCut = false;

            try
            {
                for (Int32 i = 0; i < directories.Length; ++i)
                {
                    String[] pathParts = directories[i].Split('\\');

                    if (pathParts[pathParts.Length - 1] == fileName)
                    {
                        repoContainsFile = true;
                        fullPathParts = directories[i].Split('.');
                        fullPath = directories[i];

                        break;
                    }
                }

                if (!repoContainsFile)
                {
                    Log.Print($"Failed execute requested script / exe with name {fileName}, file not found", LogSeverity.Error);
                    responseMessage = "*Unable to find file*";
                    responseColor = Color.Red;

                    goto End;
                }

                if (fullPathParts == null || fullPath == null)
                {
                    throw new InvalidDataException("path was null");
                }
            }
            catch (Exception e)
            {
                Log.Print($"An unknown error occurred while parsing the file path or name for '{fileName}'\nError: {e.Message}", LogSeverity.Error);
                responseMessage = $"*An unknown error occurred while parsing the file path or name for '{fileName}'\nError: {e.Message}*";
                responseColor = Color.Red;

                goto End;
            }

            ExecResult externalOutput;

            try
            {
                externalOutput = fullPathParts[fullPathParts.Length - 1].ToLower() switch
                {
                    "ps1" => ExecuteScript("C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe", fullPath),
                    "bat" => ExecuteScript("C:\\Windows\\System32\\cmd.exe", fullPath),
                    "exe" => ExecuteScript(fullPath, null),
                    _ => throw new InvalidDataException($"unsupported file extension: '{fullPathParts[fullPathParts.Length - 1].ToLower()}'"),
                };

                //

                if (externalOutput.errOut.Length > 3900)
                {
                    externalOutput.errOut = externalOutput.errOut.Substring(0, 3900);
                    externalOutput.errOut += "...";

                    wasCut = true;
                }

                if (externalOutput.errOut.Length + externalOutput.stdOut.Length > 3900)
                {
                    externalOutput.stdOut = externalOutput.stdOut.Substring(0, 3900 - externalOutput.errOut.Length);
                    externalOutput.stdOut += "...";

                    wasCut = true;
                }

                //

                if (externalOutput.errOut == null || externalOutput.errOut == "")
                {
                    responseColor = Color.Blue;

                    responseMessage = $"**Successfully executed {fileName}**\n\nStandard-Out: \n```{externalOutput.stdOut}```\n\nError-Out: \n```no errors encountered```\n\nExit-Code: `{externalOutput.ExitCode}`";

                    Log.Print($"Executed '{fileName}'", LogSeverity.Info);
                }
                else
                {
                    responseColor = Color.Orange;

                    if (externalOutput.stdOut == null || externalOutput.stdOut == "")
                    {
                        responseMessage = $"**Executed {fileName}**\n\nStandard-Out: \n```no output```\n\nError-Out: \n```{externalOutput.errOut}```\n\nExit-Code: `{externalOutput.ExitCode}`";
                    }
                    else
                    {
                        responseMessage = $"**Executed {fileName}**\n\nStandard-Out: \n```{externalOutput.stdOut}```\n\nError-Out: \n```{externalOutput.errOut}```\n\nExit-Code: `{externalOutput.ExitCode}`";
                    }

                    Log.Print($"Executed '{fileName}', error-out was not empty", LogSeverity.Info);
                }
            }
            catch (Exception e)
            {
                Log.Print($"An error occurred while executing a script or executable file with name '{fileName}'\nError: {e.Message}", LogSeverity.Error);
                responseMessage = $"*An error occurred while executing a script or executable file with name '{fileName}'\nError: {e.Message}*";
                responseColor = Color.Red;
            }
        End:

            if (wasCut)
            {
                responseMessage += "\n\n*message was cut, to long*";
            }

            AES_FastSocket.SendTCP(ref socket, ServerResponseBuilder(ref responseMessage, ref responseColor), CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
        }
        private static ExecResult ExecuteScript(String file, String command)
        {
            Process process = new();
            process.StartInfo.FileName = file;
            process.StartInfo.Arguments = "/c " + command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            String std = process.StandardOutput.ReadToEnd();
            String err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return new ExecResult(ref std, ref err, process.ExitCode);
        }

        private static void RemoteDownload()
        {
            Byte[] rawDownloadData = AES_FastSocket.ReceiveTCP(ref socket, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
            String downloadData = Encoding.UTF8.GetString(rawDownloadData);

            //[0] = url, [1] = fileName
            String[] downloadDataParts = downloadData.Split('§');

            String responseMessage;
            Color responseColor;

            try
            {
                using HttpResponseMessage response = http_client.GetAsync(downloadDataParts[0]).Result;
                response.EnsureSuccessStatusCode();

                Byte[] responseData = response.Content.ReadAsByteArrayAsync().Result;

                File.WriteAllBytes(Path.Combine(CurrentConfig.scriptDirectory, downloadDataParts[1]), responseData);

                responseMessage = "**Successfully uploaded file**";
                responseColor = Color.Blue;

                Log.Print($"Successfully downloaded requested file from '{downloadDataParts[0]}' with name '{downloadDataParts[1]}'", LogSeverity.Error);
            }
            catch (HttpRequestException e)
            {
                responseMessage = $"*Endpoint failed to download file*\n\n{e.Message}";
                responseColor = Color.Red;

                Log.Print($"Failed to download requested file with the following error message: {e.Message}", LogSeverity.Error);
            }

            AES_FastSocket.SendTCP(ref socket, ServerResponseBuilder(ref responseMessage, ref responseColor), CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
        }
        
        //

        private static Byte[] ServerResponseBuilder(ref String text, ref Color color)
        {
            Byte[] rawColor = BitConverter.GetBytes((UInt32)color);

            Byte[] rawText = Encoding.UTF8.GetBytes(text);

            Byte[] combined = new Byte[rawColor.Length + rawText.Length];

            rawColor.CopyTo(combined, 0);
            rawText.CopyTo(combined, 4);

            return combined;
        }
    }

    internal struct ExecResult
    {
        internal ExecResult(ref String stdOut, ref String errOut, Int64 exitCode)
        {
            this.stdOut = stdOut;
            this.errOut = errOut;
            ExitCode = exitCode;
        }

        internal String stdOut;
        internal String errOut;
        internal readonly Int64 ExitCode;
    }

    internal enum Color : UInt32
    {
        Teal = 1752220u,
        DarkTeal = 1146986u,
        Green = 3066993u,
        DarkGreen = 2067276u,
        Blue = 3447003u,
        DarkBlue = 2123412u,
        Purple = 7419530u,
        Magenta = 15277667u,
        DarkMagenta = 11342935u,
        Gold = 15844367u,
        LightOrange = 12745742u,
        Orange = 15105570u,
        DarkOrange = 11027200u,
        Red = 15158332u,
        DarkRed = 10038562u,
        LightGrey = 9936031u,
        LighterGrey = 9807270u,
        DarkGrey = 6323595u,
        DarkerGrey = 5533306u
    }
}