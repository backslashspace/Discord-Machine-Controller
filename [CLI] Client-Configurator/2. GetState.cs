using System.IO;
using System.ServiceProcess;

namespace Configurator
{
    internal static partial class Program
    {
        private static void GetState()
        {
            if (Directory.Exists(Config.InstallPath))
            {
                State.InstallDirectoryIsPresent = true;

                if (File.Exists(Config.InstallPath + Config.ServiceName + ".exe"))
                {
                    State.ServiceIsPresent = true;
                }
            }

            try
            {
                using (ServiceController sc = new(Config.ServiceName))
                {
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        State.ServiceIsRunning = true;
                    }

                    State.MMCServiceIsPresent = true;
                }
            }
            catch { }
        }
    }
}