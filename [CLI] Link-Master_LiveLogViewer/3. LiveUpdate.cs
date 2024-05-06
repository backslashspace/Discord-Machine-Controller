using System;

namespace LogViewer
{
    internal static partial class Program
    {
        private static Byte[] keepAliveBuffer = new Byte[] { 0b10101010 };
        private static void LiveUpdateLoop()
        {
            Byte[] buffer;

            while (true)
            {
                xSocket.TCP_Receive(ref socket, out buffer);

                if (buffer.Length == 1)
                {
                    xSocket.TCP_Send(ref socket, ref keepAliveBuffer);

                    continue;
                }

                ConsoleMessage logMessage = (ConsoleMessage)Deserialize(ref buffer, typeof(ConsoleMessage));

                xConsole.PrintLog(logMessage);
            }
        }
    }
}