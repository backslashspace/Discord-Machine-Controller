using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Media;

namespace Configurator
{
    public partial class AskUserForParams : Window
    {
        /// <summary>
        /// [0] client config | [1] server config string
        /// </summary>
        internal String[] Result = null;

        public AskUserForParams()
        {
            InitializeComponent();
        }

        private void Act_Click(object sender, RoutedEventArgs e)
        {
            Act.IsEnabled = false;

            Guid guid = Guid.NewGuid();
            Byte[] keyBytes = new Byte[96];
            Random random = new Random();
            random.NextBytes(keyBytes);

            String base64Keys = Convert.ToBase64String(keyBytes);

            Result = new String[2];


            Result[0] = $"scriptDirectory: \"{Script_Path.Text}\"\n" +
                $"serverPort: \"{Port.Text}\"\n" +
                $"serverIP: \"{IPAddress.Parse(IP.Text)}\"\n" +
                $"channelID: \"{channelID.Text}\"\n" +
                $"guid: \"{guid}\"\n" +
                $"keys: \"{base64Keys}\"";

            //

            Result[1] = $"vmChannelLink: \"{Alias.Text}:{guid}:{channelID.Text}:{base64Keys}\"";

            Close();
        }

        private static String green = "#70b04d";
        private static String red = "#ae3127";

        private Boolean _Alias = false;
        private Boolean _Port = false;
        private Boolean _ID = false;
        private Boolean _IP = false;
        private Boolean _Path = false;

        private void Alias_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (Alias.Text.IndexOf(' ') == -1)
            {
                _Alias = true;

                SetButtonState();

                Alias.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(green));

                return;
            }

            Alias.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(red));

            _Alias = false;
        }

        private void IP_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (IPAddress.TryParse(IP.Text, out IPAddress ip))
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _IP = true;

                    SetButtonState();

                    IP.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(green));

                    return;
                }
            }

            IP.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(red));

            _IP = false;
        }

        private void Port_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (UInt16.TryParse(Port.Text, out _))
            {
                _Port = true;

                SetButtonState();

                Port.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(green));

                return;
            }

            Port.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(red));

            _Port = false;
        }

        private void ID_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (UInt64.TryParse(channelID.Text, out _))
            {
                _ID = true;

                SetButtonState();

                channelID.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(green));

                return;
            }

            channelID.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(red));

            _ID = false;
        }

        private void SetButtonState()
        {
            if (!_Port)
            {
                if (Act != null)
                {
                    Act.IsEnabled = false;
                }

                return;
            }

            if (!_ID)
            {
                if (Act != null)
                {
                    Act.IsEnabled = false;
                }

                return;
            }

            if (!_Alias)
            {
                if (Act != null)
                {
                    Act.IsEnabled = false;
                }

                return;
            }

            if (!_IP)
            {
                if (Act != null)
                {
                    Act.IsEnabled = false;
                }

                return;
            }

            if (!_Path)
            {
                if (Act != null)
                {
                    Act.IsEnabled = false;
                }

                return;
            }

            if (Act != null)
            {
                Act.IsEnabled = true;
            }
        }

        private void PathTest(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(Script_Path.Text))
            {
                _Path = true;

                SetButtonState();

                Script_Path.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(green));

                return;
            }

            Script_Path.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(red));

            _Path = false;
        }
    }
}