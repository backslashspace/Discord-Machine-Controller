using System.Reflection;
using System;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace LinkSlave.Win
{
    internal static class Program
    {
        internal static String Version;

        static void Main()
        {
            Version = Assembly.GetEntryAssembly().GetName().Version.ToString();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}