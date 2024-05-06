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

        internal static xVersion AssemblyVersion;
        internal static xVersion AssemblyFileVersion;
        internal static String AssemblyInformationalVersion;

        internal static Stopwatch LoadTime = new();

        static void Main()
        {
            LoadTime.Start();

            GetVersionInfos();

            AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ServiceName = $"Discord Link-Master {AssemblyInformationalVersion} ({AssemblyFileVersion}";

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
            AssemblyFileVersion = new((UInt32)fileVersionInfo.FileMajorPart, (UInt32)fileVersionInfo.FileMinorPart, (UInt32)fileVersionInfo.FileBuildPart, (UInt32)fileVersionInfo.FilePrivatePart);
            AssemblyInformationalVersion = fileVersionInfo.ProductVersion;
        }
    }
}