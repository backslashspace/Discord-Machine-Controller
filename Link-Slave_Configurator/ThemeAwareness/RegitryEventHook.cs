using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Configurator
{
    public partial class MainWindow
    {
        private static void WinThemeRegChangeEventHook()
        {
            WindowsIdentity currentUser = WindowsIdentity.GetCurrent();

            String valueName = "AccentPalette";
            String keyPath = @"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Accent";

            WqlEventQuery query = new($@"SELECT * FROM RegistryValueChangeEvent WHERE Hive='HKEY_USERS' AND KeyPath='{currentUser.User.Value}\\{keyPath}' AND ValueName='{valueName}'");

            ManagementEventWatcher watcher = new(query);

            watcher.EventArrived += ThemeChanged;

            watcher.Start();
        }
    }
}