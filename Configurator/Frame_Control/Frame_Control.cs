using System;
using System.Windows.Media;
using System.Windows;

namespace Configurator
{
    public partial class MainWindow
    {
        #region Window Head Button Logic

        #region Minimize_Window
        private void Minimize_Button_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Minimize_Button_Mouse_Is_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Minimize_Button_Color(WindowButtonColors.Minimize_Button_Color_Mouse_Is_Over);
        }

        private void Minimize_Button_Mouse_Is_Not_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Minimize_Button_Color(WindowButtonColors.Minimize_Button_Color_Idle);
        }

        private void Minimize_Button_Down(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Minimize_Button_Color(WindowButtonColors.Minimize_Button_Color_Down);
        }

        //

        private void Update_Minimize_Button_Color(String NewHexColor)
        {
            Minimize_Button_Button_Background.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(NewHexColor));
        }
        #endregion

        #region Close_Button
        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Close_Button_Mouse_Is_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Close_Button_Color(WindowButtonColors.Close_Button_Color_Mouse_Is_Over);
        }

        private void Close_Button_Mouse_Is_Not_Over(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Close_Button_Color(WindowButtonColors.Close_Button_Color_Idle);
        }

        private void Close_Button_Down(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Update_Close_Button_Color(WindowButtonColors.Close_Button_Color_Down);
        }

        //

        private void Update_Close_Button_Color(String NewHexColor)
        {
            Close_Button_Background.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(NewHexColor));
        }
        #endregion

        #endregion

        //# # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # # #

        private static class WindowButtonColors
        {
            public static String Minimize_Button_Color_Idle = "#202020";
            public static String Minimize_Button_Color_Mouse_Is_Over = "#2d2d2d";
            public static String Minimize_Button_Color_Down = "#2a2a2a";
            public static String Minimize_Button_Stroke_Color_Enabled = "#ffffff";
            public static String Minimize_Button_Stroke_Color_Disabled = "#777777";

            public static String Close_Button_Color_Idle = "#202020";
            public static String Close_Button_Color_Mouse_Is_Over = "#c42b1c";
            public static String Close_Button_Color_Down = "#b22a1b";
            public static String Close_Button_Stroke_Color_Enabled = "#ffffff";
            public static String Close_Button_Stroke_Color_Disabled = "#777777";
        }
    }
}