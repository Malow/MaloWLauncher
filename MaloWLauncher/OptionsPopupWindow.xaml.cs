using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace MaloWLauncher
{
    public partial class OptionsPopupWindow : Window
    {
        public OptionsPopupWindow()
        {
            InitializeComponent();
        }
        
        private void Download_Clicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void OnLaunchParamDropDownChanged(object sender, SelectionChangedEventArgs e)
        {
            // this is ugly and probably should exist in a file somewhere
            string launchString = "";
            switch (LaunchParamDropDown.SelectedIndex)
            {
                case 0:
                    launchString = @"\dx9";
                    break;
                case 1:
                    launchString = @"\dx11";
                    break;
                case 2:
                    launchString = @"\win8";
                    break;
            }
            HelperFunctions.UpdateLaunchParameters(launchString);
        }

        private void SetGameLocation_Clicked(object sender, RoutedEventArgs e)
        {
            HelperFunctions.OpenFileBrowserAndSetConfigGameLocation();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            string launchParams = HelperFunctions.GetLaunchParameters();
            switch(launchParams)
            {
                case @"\dx9":
                    LaunchParamDropDown.SelectedValue = "DirectX 9";
                    break;
                case @"\dx11":
                    LaunchParamDropDown.SelectedValue = "DirectX 10 & 11";
                    break;
                case @"\win8":
                    LaunchParamDropDown.SelectedValue = "Windows 8 - Touch Enabled";
                    break;
            }
        }
    }
}
