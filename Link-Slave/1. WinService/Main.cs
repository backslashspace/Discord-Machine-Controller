using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Link_Slave
{
    internal static partial class Program
    {
        internal static String AssemblyPath;
        internal static String ServiceName;
        internal static xVersion Version;

        static void Main()
        {
            AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Version = new(Assembly.GetExecutingAssembly().GetName().Version);
            ServiceName = $"Discord Link-Slaver v{Version}";

            ServiceBase[] service = new[]
            {
                new Service()
            };
            ServiceBase.Run(service);

            Environment.Exit(0);
        }
    }
}