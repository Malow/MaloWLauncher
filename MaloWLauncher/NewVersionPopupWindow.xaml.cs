using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class NewVersionPopupWindow : Window
    {
        private string downloadUrl;

        public NewVersionPopupWindow(string downloadUrl)
        {
            InitializeComponent();
            this.downloadUrl = downloadUrl;
        }

        private void Download_Clicked(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(this.downloadUrl));
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                this.Close();
                this.Owner.Activate();
                System.Windows.Application.Current.Shutdown();
            }));
        }

        void Window_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                System.Windows.Application.Current.Shutdown();
            }));
        }
    }
}
