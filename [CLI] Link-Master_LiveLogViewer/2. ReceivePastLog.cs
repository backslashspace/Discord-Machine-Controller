using System;
using System.Collections.Generic;

namespace LogViewer
{
    internal static partial class Program
    {
        private static List<ConsoleMessage> ReceivePastLog()
        {
            xSocket.TCP_Receive(ref socket, out Byte[] buffer);

            return (List<ConsoleMessage>)Deserialize(ref buffer, typeof(List<ConsoleMessage>));
        }
    }
}