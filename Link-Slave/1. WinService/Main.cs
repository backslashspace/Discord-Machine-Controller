using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Link_Slave
{
    internal static partial class Program
    {
        internal static String assemblyPath;
        internal static String serviceName;
        internal static xVersion version;

        static void Main()
        {
            assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            version = new(Assembly.GetExecutingAssembly().GetName().Version);
            serviceName = $"Discord Link-Slaver v{version}";

            ServiceBase[] service = new[]
            {
                new Service()
            };
            ServiceBase.Run(service);

            Environment.Exit(0);
        }
    }
}