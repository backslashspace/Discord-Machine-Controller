using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        internal static Socket socket;

        internal static void WorkingLoop()
        {
            for (Byte retries = 0; retries < 5; ++retries)
            {
                try
                {
                    while (!WorkerThread.Worker_WasCanceled)
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
                    if (retries == 4)
                    {
                        Log.FastLog("Main-Worker", $"At least 5 total errors occurred in in the main worker thread, shutting down service", xLogSeverity.Critical);

                        Control.Shutdown.ServiceComponents();
                    }

                    Log.FastLog("Main-Worker", $"An error occurred in the main worker thread, this was the {retries + 1} out of 5 allowed errors, the error message was:\n" +
                        $"{ex.Message}\n\n\t=> continuing", xLogSeverity.Error);
                }
            }
        }

        private static void HandleErrorCode(ref Byte exitCode)
        {
            switch (exitCode)
            {
                case 1:
                    Log.FastLog("Main-Worker", "Lost connection", xLogSeverity.Info);
                    return;

                default:
                    throw new InvalidOperationException("unknown error code");
            }
        }
    }
}