using System;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace Configurator.Pages
{
    public partial class NotInstalled : UserControl
    {
        public NotInstalled()
        {
            InitializeComponent();

            Loaded += LoadData;
        }

        //

        private void Continue(object sender, RoutedEventArgs e)
        {
            MainWindow._ConfirmEnslavement.Visibility = Visibility.Visible;
            MainWindow._NotInstalled.Visibility = Visibility.Collapsed;
        }

        //

        private void LoadData(object sender, EventArgs e)
        {
            String field = "Service not found\n" +
                $"OS: {Environment.OSVersion}\n" +
                $"Machine name: {Environment.MachineName}\n" +
                $"Current user name: {Environment.UserName}\n" +
                $"IP: {GetIPInfo()}\n";

            NotInstalled_InfoBox.Text = field;
        }

        private static String GetIPInfo()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                String text = "";
                Boolean noComma = true;

                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        if (noComma)
                        {
                            text += $"{ip}";

                            noComma = false;
                        }
                        else
                        {
                            text += $", {ip}";
                        }
                    }
                }

                return text;
            }
            else
            {
                return "not connected";
            }
        }
    }
}