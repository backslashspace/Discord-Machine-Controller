using EXT.System.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Configurator.Pages
{
    public partial class ConfirmEnslavement : UserControl
    {
        public ConfirmEnslavement()
        {
            InitializeComponent();
        }

        private async void Install(object sender, RoutedEventArgs e)
        {
            MainWindow._ServerConfigDisplay.Visibility = Visibility.Visible;
            MainWindow._ConfirmEnslavement.Visibility = Visibility.Collapsed;

            ServerConfigDisplay.OptionalBox.Text = await Link.InstallLink();

            ServerConfigDisplay.OptionalBox.Text += "starting service\n";

            xService.Start("Discord VMLink Slave");
            
            await Task.Delay(1000);

            ServerConfigDisplay.OptionalBox.Text += $"Status is {xService.GetStatusString("Discord VMLink Slave")}";
        }
    }
}