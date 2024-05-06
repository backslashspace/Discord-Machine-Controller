using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace LogViewer
{
    internal static partial class Program
    {
        internal static String Version;
        internal static Socket socket;

        internal readonly static String ProgramName = "Link-Master Log Viewer";

        private static void Main()
        {
            Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            Console.Title = $"{ProgramName} v{Version}";

            xConsole.PrintLogo();

            IPEndPoint remoteEndpoint = new(IPAddress.Loopback, 3001);
            
            BuildSocket();

            while (true)
            {
                try
                {
                    Connect(ref remoteEndpoint);

                    List<ConsoleMessage> pastLog = ReceivePastLog();

                    for (Int32 i = 0; i < pastLog.Count; ++i)
                    {
                        xConsole.PrintLog(pastLog[i]);
                    }

                    LiveUpdateLoop();
                }
                catch
                {
                    ConsoleMSG("> Lost connection", ConsoleColor.DarkRed);

                    Task.Delay(2048).Wait();
                    
                    try
                    {
                        socket.Close(0);
                    }
                    catch { }

                    BuildSocket();
                }
            }
        }

        //

        private static void BuildSocket()
        {
            socket = new(IPAddress.Loopback.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Blocking = true;
            socket.NoDelay = true;
            socket.ReceiveTimeout = 4096;
            socket.SendTimeout = 4096;
        }

        private static void Connect(ref IPEndPoint remoteEndpoint)
        {
            ConsoleMSG("> Attempting to connect to service\n", ConsoleColor.DarkGreen);

            while (true)
            {
                try
                {
                    socket.Connect(remoteEndpoint);

                    break;
                }
                catch
                {
                    Task.Delay(512).Wait();
                }
            }
        }

        private static void ConsoleMSG(String msg, ConsoleColor msgColor)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("\n[");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("Console");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("] ");
            Console.ForegroundColor = msgColor;
            Console.WriteLine(msg);
        }
    }
}