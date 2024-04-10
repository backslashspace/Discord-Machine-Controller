using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Configurator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Pin.MainWindow = this;
            Pin.Dispatcher = Dispatcher;

            GetAppInfo();

            Title = "LCC v" + AppInfo.AssemblyFileVersion.ToString();
            Window_Title.Text = "Link-Client Configurator v" + AppInfo.AssemblyInformationalVersion;
        }

        private static void GetAppInfo()
        {
            AppInfo.AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            AppInfo.AssemblyVersion = new(Assembly.GetExecutingAssembly().GetName().Version);

            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            AppInfo.AssemblyFileVersion = new((UInt16)fileVersionInfo.FileMajorPart, (UInt16)fileVersionInfo.FileMinorPart, (UInt16)fileVersionInfo.FileBuildPart, (UInt16)fileVersionInfo.FilePrivatePart);
            AppInfo.AssemblyInformationalVersion = fileVersionInfo.ProductVersion;
        }
    }
}