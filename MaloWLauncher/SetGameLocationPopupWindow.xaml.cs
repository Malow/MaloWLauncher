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
    public partial class SetGameLocationPopupWindow : Window
    {
        public SetGameLocationPopupWindow()
        {
            InitializeComponent();
        }

        private void Browse_Clicked(object sender, RoutedEventArgs e)
        {
            HelperFunctions.OpenFileBrowserAndSetConfigGameLocation();
            e.Handled = true;
            Application.Current.Dispatcher.Invoke(new Action(() => 
            {
                this.Close();
                this.Owner.Activate();
            }));
        }
    }
}
