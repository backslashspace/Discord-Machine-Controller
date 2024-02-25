using System.Reflection;
using System;
using System.ServiceProcess;
using System.IO;

namespace DC_SRV_VM_LINK.Service
{
    internal static class Program
    {
        internal static String assemblyPath;

        internal const String SRV_Name = "Discord VM-Link Master";

        internal static String Version;

        internal static void Main()
        {
            assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            Version = Assembly.GetEntryAssembly().GetName().Version.ToString();

            ServiceBase[] service;

            service = new ServiceBase[]
            {
                new Service()
            };

            ServiceBase.Run(service);
        }
    }
}