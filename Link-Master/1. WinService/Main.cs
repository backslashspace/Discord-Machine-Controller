using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Link_Master
{
    internal static partial class Program
    {
        internal static String assemblyPath;
        internal static String serviceName;
        internal static String version;

        static void Main()
        {
            assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            serviceName = $"Discord Link-Master v{version}";

            ServiceBase[] service = new[]
            {
                new Service()
            };
            ServiceBase.Run(service);

            Environment.Exit(0);
        }
    }
}