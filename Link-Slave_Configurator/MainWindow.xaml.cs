using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Configurator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WinThemeRegChangeEventHook();
            InitializeComponent();
            GetAppInfo();

            Pin.MainWindow = this;
            Pin.Dispatcher = Dispatcher;

            Title = "LC v" + AppInfo.AssemblyFileVersion.ToString();
            Window_Title.Text = "Link Configurator v" + AppInfo.AssemblyInformationalVersion;  
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