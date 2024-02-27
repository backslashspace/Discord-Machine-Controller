using System.Reflection;
using System;
using System.ServiceProcess;

namespace LinkSlave.Win
{
    internal static class Program
    {
        internal static String Version;

        static void Main()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}