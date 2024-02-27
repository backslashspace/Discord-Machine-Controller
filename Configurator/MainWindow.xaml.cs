using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Configurator.Pages;

//
using EXT.System.Service;

namespace Configurator
{
    public partial class MainWindow : Window
    {
        internal static Dispatcher UIDispatcher;

        internal static UserControl _NotInstalled;
        internal static UserControl _ConfirmEnslavement;
        internal static UserControl _AlreadyInstalled;
        internal static UserControl _ServerConfigDisplay;

        public MainWindow()
        {
            InitializeComponent();

            UIDispatcher = Dispatcher.CurrentDispatcher;

            Loaded += LoadData;

            _NotInstalled = NotInstalled;
            _ConfirmEnslavement = ConfirmEnslavement;
            _AlreadyInstalled = AlreadyInstalled;
            _ServerConfigDisplay = ServerConfigDisplay;
        }

        //

        private void LoadData(object sender, EventArgs e)
        {
            Window_Title.Text = $"VM Link Generator v{Assembly.GetExecutingAssembly().GetName().Version}";

            Byte state = 0;

            try
            {
                xService.GetStartupType("Discord VMLink Slave");
            }
            catch
            {
                ++state;
            }

            if (!File.Exists("C:\\Program Files\\Discord VMLink Slave\\LinkSlave.exe"))
            {
                ++state;
            }

            if (state < 2) 
            {
                AlreadyInstalled.Visibility = Visibility.Visible;
                NotInstalled.Visibility = Visibility.Collapsed;
            }

            if (state == 2)
            {
                
            }
        }
    }
}