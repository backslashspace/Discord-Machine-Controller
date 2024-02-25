using EXT.System.Service;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Configurator.Pages
{
    public partial class AlreadyInstalled : UserControl
    {
        public AlreadyInstalled()
        {
            InitializeComponent();

            Loaded += LoadData;
        }

        //

        private void LoadData(object sender, EventArgs e)
        {
            String version = "not found";

            if (File.Exists("C:\\Program Files\\Discord VMLink Slave\\LinkSlave.exe"))
            {
                version = FileVersionInfo.GetVersionInfo("C:\\Program Files\\Discord VMLink Slave\\LinkSlave.exe").FileVersion;
            }
            else
            {

            }

            String serviceStatus = "";
            String serviceStartType = "";

            try
            {
                serviceStatus = xService.GetStatusString("Discord VMLink Slave");
                serviceStartType = xService.GetStartupType("Discord VMLink Slave");

                AlreadyInstalled_InfoBox.Text = $"LinkSlave.exe version: {version}\n" +
                $"Status: {serviceStatus}\n" +
                $"Start type: {serviceStartType}";
            }
            catch
            {
                AlreadyInstalled_InfoBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#aa0000"));
                AlreadyInstalled_InfoBox.Text = "ERROR: Service 'Discord VMLink Slave' not registered!";
            }
        }

        //

        private async void NewConfig(object sender, RoutedEventArgs e)
        {
            await Link.GenerateConfig();

            MainWindow._ServerConfigDisplay.Visibility = Visibility.Visible;
            MainWindow._AlreadyInstalled.Visibility = Visibility.Collapsed;
        }

        private async void Uninstall_Button(object sender, RoutedEventArgs e)
        {
            Dialogue msg = new("Uninstall", "Please confirm that you want to free this machine", Dialogue.Icons.Shield_Exclamation_Mark, "Cancel", "Uninstall");
        
            msg.ShowDialog();

            if (msg.Result == 1)
            {
                Boolean success = await Uninstall.Commit();

                if (success)
                {
                    Dialogue tell = new("Uninstall", "Successfully removed link", Dialogue.Icons.Tick, "OK");

                    tell.ShowDialog();

                    Environment.Exit(0);
                }
                else
                {
                    AlreadyInstalled_InfoBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff0000"));
                    AlreadyInstalled_InfoBox.Text = "ERROR: Unable to uninstall service\n\nTerminating...";

                    await Task.Delay(10240);

                    Environment.Exit(1);
                }
            }
        }
    }
}