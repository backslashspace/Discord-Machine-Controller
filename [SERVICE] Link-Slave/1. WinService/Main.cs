using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;

namespace Link_Slave
{
    internal static partial class Program
    {
        internal static String AssemblyPath;
        internal static String ServiceName;

        internal static xVersion AssemblyVersion;
        internal static xVersion AssemblyFileVersion;
        internal static String AssemblyInformationalVersion;

        static void Main()
        {
            AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            GetVersionInfos();
            ServiceName = $"Discord Link-Slave {AssemblyInformationalVersion}s ({AssemblyFileVersion}";

            ServiceBase[] service = new[]
            {
                new Service()
            };
            ServiceBase.Run(service);

            Environment.Exit(0);
        }

        private static void GetVersionInfos()
        {
            AssemblyVersion = new(Assembly.GetExecutingAssembly().GetName().Version);

            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            AssemblyFileVersion = new((UInt16)fileVersionInfo.FileMajorPart, (UInt16)fileVersionInfo.FileMinorPart, (UInt16)fileVersionInfo.FileBuildPart, (UInt16)fileVersionInfo.FilePrivatePart);
            AssemblyInformationalVersion = fileVersionInfo.ProductVersion;
        }
    }
}