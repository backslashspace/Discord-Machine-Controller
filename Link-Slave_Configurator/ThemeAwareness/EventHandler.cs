using System;
using System.Diagnostics;
using System.Management;
using System.Windows.Media;

namespace Configurator
{
    public partial class MainWindow
    {
        private static void ThemeChanged(object sender, EventArrivedEventArgs e)
        {
            Debug.WriteLine("registry event fired");

            Byte[] rawPalette;

            try
            {
                rawPalette = (Byte[])Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Accent", "AccentPalette", null);
            }
            catch
            {
                Debug.WriteLine("invalid reg type");

                return;
            }

            if (rawPalette == null || rawPalette.Length != 32)
            {
                if (rawPalette == null)
                {
                    Debug.WriteLine("new reg data returned null");
                }
                else
                {
                    Debug.WriteLine($"new reg data had invalid length, expected 32, found {rawPalette.Length}");
                }

                return;
            }

            CurrentColors.RawPalette = rawPalette;

            //App.Current.Resources["PrimaryButton_Background"] = new SolidColorBrush(Color.FromRgb(rawPalette[12], rawPalette[13], rawPalette[14]));
        }
    }
}