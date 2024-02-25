using EXT.System.Service;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class ServerConfigDisplay : UserControl
    {
        internal static TextBox serverStringBox;
        internal static TextBlock OptionalBox;

        public ServerConfigDisplay()
        {
            InitializeComponent();

            serverStringBox = ServerString;
            OptionalBox = Optional;
        }

        private void Act_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}