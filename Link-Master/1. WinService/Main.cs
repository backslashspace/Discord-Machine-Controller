using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Link_Master
{
    internal static partial class Program
    {
        internal static String AssemblyPath;
        internal static String ServiceName;
        internal static xVersion Version;

        internal static Stopwatch LoadTime = new();

        static void Main()
        {
            LoadTime.Start();

            AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Version = new(Assembly.GetExecutingAssembly().GetName().Version);
            ServiceName = $"Discord Link-Master v{Version}";

            ServiceBase[] service = new[]
            {
                new Service()
            };
            ServiceBase.Run(service);

            Environment.Exit(0);
        }
    }
}