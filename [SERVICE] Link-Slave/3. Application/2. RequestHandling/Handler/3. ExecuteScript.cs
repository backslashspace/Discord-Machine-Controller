using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
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

            String stdOut;
            String errOut;
            Int32 exitCode;

            try     // run
            {
                String[] fileParts = fileName.Split('.');
                (stdOut, errOut, exitCode) = fileParts[fileParts.Length - 1].ToLower() switch
                {
                    "ps1" => RunPS(CurrentConfig.ScriptDirectory + "\\" + fileName),
                    "bat" => Run("C:\\Windows\\System32\\cmd.exe", CurrentConfig.ScriptDirectory + "\\" + fileName),
                    "exe" => Run(CurrentConfig.ScriptDirectory + "\\" + fileName, null),
                    _ => throw new InvalidDataException($"unsupported file extension: '{fileParts[fileParts.Length - 1].ToLower()}'"),
                };

                stdOut = stdOut.Trim();
                errOut = errOut.Trim();
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

            (formattedResult, Boolean wasCut) = BuildResponse(ref stdOut, ref errOut, ref exitCode, ref fileName);

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

        private static (String stdOut, String errOut, Int32 exitCode) Run(String path, String args)
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

            String stdOut = process.StandardOutput.ReadToEnd();
            String errOut = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Int32 exitCode = process.ExitCode;

            return (stdOut, errOut, exitCode);
        }

        private static (String stdOut, String errOut, Int32 exitCode) RunPS(String path)
        {
            using StreamReader streamReader = new(path, true);
            String scriptContent = streamReader.ReadToEnd();
            streamReader.Close();

            (String stdOut, String errOut) = PSInvoke.Script(ref scriptContent);

            if (errOut == "")
            {
                return (stdOut, errOut, 0);
            }
            else
            {
                return (stdOut, errOut, -1);
            }   
        }

        private static class PSInvoke
        {
            internal static (String stdOut, String errOut) Script(ref readonly String script)
            {
                InitialSessionState session = InitialSessionState.CreateDefault();
                Runspace runSpace = RunspaceFactory.CreateRunspace(session);
                runSpace.Open();

                PowerShell command = PowerShell.Create();
                command.Runspace = runSpace;
                String errOut = "";
                String stdOut = "";

                IEnumerable<PSObject> results = command.AddScript(script).Invoke();

                foreach (PSObject obj in results)
                {
                    if (obj.BaseObject is String)
                    {
                        stdOut += $"{obj.BaseObject}\n";
                    }
                }

                foreach (ErrorRecord errorRecord in command.Streams.Error)
                {
                    errOut += $"{errorRecord.Exception.Message}\n";
                }

                command.Dispose();
                runSpace.Dispose();

                return (stdOut, errOut);
            }
        }

        private static (String output, Boolean wasCut) BuildResponse(ref String stdOut, ref String errOut, ref Int32 exitCode, ref String fileName)
        {
            Boolean wasCut = false;

            if (errOut.Length > 3900)
            {
                errOut = errOut.Substring(0, 3900);
                errOut += "...";

                wasCut = true;
            }

            if (errOut.Length + stdOut.Length > 3900)
            {
                stdOut = stdOut.Substring(0, 3900 - errOut.Length);
                stdOut += "...";

                wasCut = true;
            }

            //

            if (errOut == null || errOut == "")
            {
                errOut = "\n-";

                Log.FastLog("ExecuteScript", $"Executed '{fileName}'", xLogSeverity.Info);
            }
            else
            {
                try
                {
                    //workaround for discord
                    //discord hides the first line if it has the following pattern in a multiline code block (for example ```test\nText```) -> 'test' will be hidden
                    Match match = Regex.Match(errOut.Split('\n')[0], DiscordFirstLineWorkaround, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        errOut = "\n" + errOut;
                    }
                }
                catch { }

                Log.FastLog("ExecuteScript", $"Executed '{fileName}', error-out was not empty", xLogSeverity.Info);
            }

            if (stdOut == null || stdOut == "")
            {
                stdOut = "\n-";
            }
            else
            {
                try
                {
                    //workaround for discord
                    //discord hides the first line if it has the following pattern in a multiline code block (for example ```test\nText```) -> 'test' will be hidden
                    Match match = Regex.Match(stdOut.Split('\n')[0], DiscordFirstLineWorkaround, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        stdOut = "\n" + stdOut;
                    }
                }
                catch { }
            }

            return ($"**Executed {fileName}**\nStandard-Out:\n```{stdOut}```\nError-Out:\n```{errOut}```\nExit-Code: `{exitCode}`", wasCut);
        }
    }
}