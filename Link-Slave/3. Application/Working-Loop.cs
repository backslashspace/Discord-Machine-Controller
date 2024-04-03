using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Link_Slave.Worker
{
    internal static partial class Client
    {
        private static Socket socket;

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
                        
                        if (Authenticate())
                        {
                            //unsuccessful

                            if (!WorkerThread.Worker_WasCanceled)
                            {
                                Task.Delay(8192).Wait();
                            }

                            continue;   
                        }
                        
                        RequestHandlerLoop();

                        // -> lost connection

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
                        $"{ex.Message}\n\nSource: {ex.Source}\n\nStackTrace: {ex.StackTrace}\n\n\t=> continuing", xLogSeverity.Error);
                }
            }
        }
    }
}