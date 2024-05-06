using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static void ExecuteScript(ref Byte errorCode, ref Byte[] buffer)
        {
            String fileName = "";
            Byte[] rawResponse;

            String formattedResult;
            Color responseColor;

            if (buffer.Length < 2)
            {
                formattedResult = "Failed to execute script file - Command data containing the target file name was missing";
                responseColor = Color.Red;

                goto SEND;
            }

            fileName = Encoding.UTF8.GetString(buffer, 1, buffer.Length - 1).Trim();
            
            if (!File.Exists(CurrentConfig.ScriptDirectory + "\\" + fileName))
            {
                formattedResult = $"Failed to execute script file\nFile: `{fileName}` not found";
                responseColor = Color.Red;
                goto SEND;
            }

            String stdOUT;
            String errOUT;
            Int32 exitCode;

            try//run
            {
                String[] fileParts = fileName.Split('.');
                (stdOUT, errOUT, exitCode) = fileParts[fileParts.Length - 1].ToLower() switch
                {
                    "ps1" => Run("C:\\Windows\\System32\\WindowsPowerShell\\v1.0\\powershell.exe", CurrentConfig.ScriptDirectory + "\\" + fileName),
                    "bat" => Run("C:\\Windows\\System32\\cmd.exe", CurrentConfig.ScriptDirectory + "\\" + fileName),
                    "exe" => Run(CurrentConfig.ScriptDirectory + "\\" + fileName, null),
                    _ => throw new InvalidDataException($"unsupported file extension: '{fileParts[fileParts.Length - 1].ToLower()}'"),
                };

                stdOUT = stdOUT.Trim();
                errOUT = errOUT.Trim();
            }
            catch (Exception ex)
            {
                if (ex.InnerException == null)
                {
                    formattedResult = $"Failed to execute file with the following error message:\n{ex.Message}";
                }
                else
                {
                    formattedResult = $"Failed to execute file with the following error message:\n{ex.InnerException.Message}";
                }

                responseColor = Color.Red;

                goto SEND;
            }

            (formattedResult, Boolean wasCut) = BuildResponse(ref stdOUT, ref errOUT, ref exitCode, ref fileName);

            Log.FastLog("Main-Worker", $"Successfully executed and send output of [{fileName}] back to master service", xLogSeverity.Info);

            if (wasCut)
            {
                responseColor = Color.Orange;

                formattedResult += "\n*message was cut, too long*";
            }
            else
            {
                responseColor = Color.Blue;
            }

        SEND:
            rawResponse = ServerResponseBuilder(ref formattedResult, ref responseColor);

            AES_TCP.RefSend(ref errorCode, ref socket, ref rawResponse, CurrentConfig.AES_Key, CurrentConfig.HMAC_Key);
        }

        //

        private static (String stdOUT, String errOUT, Int32 exitCode) Run(String path, String args)
        {
            Process process = new();
            process.StartInfo.FileName = path;

            if (args != null)
            {
                process.StartInfo.Arguments = "/c " + args;
            }

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            String stdOUT = process.StandardOutput.ReadToEnd();
            String errOUT = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Int32 exitCode = process.ExitCode;

            return (stdOUT, errOUT, exitCode);
        }

        private static (String output, Boolean wasCut) BuildResponse(ref String stdOUT, ref String errOUT, ref Int32 exitCode, ref String fileName)
        {
            Boolean wasCut = false;

            if (errOUT.Length > 3900)
            {
                errOUT = errOUT.Substring(0, 3900);
                errOUT += "...";

                wasCut = true;
            }

            if (errOUT.Length + stdOUT.Length > 3900)
            {
                stdOUT = stdOUT.Substring(0, 3900 - errOUT.Length);
                stdOUT += "...";

                wasCut = true;
            }

            //

            if (errOUT == null || errOUT == "")
            {
                errOUT = "\n-";

                Log.FastLog("ExecuteScript", $"Executed '{fileName}'", xLogSeverity.Info);
            }
            else
            {
                try
                {
                    //workaround for discord
                    //discord hides the first line if it has the following pattern in a multiline code block (for example ```test\nText```) -> 'test' will be hidden
                    Match match = Regex.Match(errOUT.Split('\n')[0], DiscordFirstLineWorkaround, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        errOUT = "\n" + errOUT;
                    }
                }
                catch { }

                Log.FastLog("ExecuteScript", $"Executed '{fileName}', error-out was not empty", xLogSeverity.Info);
            }

            if (stdOUT == null || stdOUT == "")
            {
                stdOUT = "\n-";
            }
            else
            {
                try
                {
                    //workaround for discord
                    //discord hides the first line if it has the following pattern in a multiline code block (for example ```test\nText```) -> 'test' will be hidden
                    Match match = Regex.Match(stdOUT.Split('\n')[0], DiscordFirstLineWorkaround, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        stdOUT = "\n" + stdOUT;
                    }
                }
                catch { }
            }

            return ($"**Executed {fileName}**\nStandard-Out:\n```{stdOUT}```\nError-Out:\n```{errOUT}```\nExit-Code: `{exitCode}`", wasCut);
        }
    }
}