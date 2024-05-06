using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        internal static Socket socket;

        internal const Byte MaxErrorCount = 4;
        internal static Byte errorCounter = 0;
        internal static Boolean errorExit = false;

        const String DiscordFirstLineWorkaround = "^\\S+\\r*$";

        internal static void WorkingLoop()
        {
            while (!errorExit)
            {
                try
                {
                    while (!WorkerThread.Worker_WasCanceled)
                    {
                        if (socket == null || !socket.Connected)
                        {
                            CreateSocket();

                            if (!Connect())
                            {
                                continue;
                            }

                            if (!Authenticate())
                            {
                                //unsuccessful

                                if (!WorkerThread.Worker_WasCanceled)
                                {
                                    Task.Delay(8192).Wait();
                                }

                                continue;
                            }

                            if (!WaitForReady())
                            {
                                Task.Delay(8192).Wait();

                                continue;
                            }
                        }                        

                        Byte exitCode = RequestHandlerLoop();

                        if (exitCode != 0)
                        {
                            HandleErrorCode(ref exitCode);
                        }

                        DisposeSocket();
                    }

                    return;     //canceled
                }
                catch (Exception ex)
                {
                    ++errorCounter;

                    if (errorCounter == 4)
                    {
                        ErrorExit();
                    }

                    Log.FastLog("Main-Worker", $"An error occurred in the main worker thread, this was the {errorCounter + 1} out of 5 allowed errors, the error message was:\n" +
                        $"{ex.Message}\n\n\t=> continuing", xLogSeverity.Error);
                }
            }
        }

        private static void HandleErrorCode(ref Byte exitCode)
        {
            switch (exitCode)
            {
                case 1:
                    Log.FastLog("Main-Worker", "Lost connection", xLogSeverity.Warning);
                    return;

                default:
                    throw new InvalidOperationException("unknown error code");
            }
        }

        private static void ErrorExit()
        {
            Log.FastLog("Main-Worker", $"At least 5 total errors occurred in in the main worker thread, shutting down service", xLogSeverity.Critical);

            Control.Shutdown.ServiceComponents();
        }
    }
}