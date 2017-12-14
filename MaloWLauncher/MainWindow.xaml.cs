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
using System.Threading;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Net;
using System.IO.Compression;
using static MaloWLauncher.ModList;

namespace MaloWLauncher
{    
    public partial class MainWindow : Window
    {
        ObservableCollection<ModModel> modModels = new ObservableCollection<ModModel>();
        ProgressPopupWindow progressPopupWindow;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ModsList.ItemsSource = modModels;
        }

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            UpdateModsList(sender, e);
            this.VersionLabel.Content = "v" + Globals.VERSION;
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // A hackfix to be able to have content right-aligned in the last column of the grid
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
            var col1 = 0.33;
            var col2 = 0.33;
            var col3 = 0.34;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
        }

        private void Refresh_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateModsList(sender, e);
        }

        private void RemoveAllMods_Clicked(object sender, RoutedEventArgs e)
        {
            HelperFunctions.UpdateToMod(null);
        }

        private void LaunchCiv5_Clicked(object sender, RoutedEventArgs e)
        {
            HelperFunctions.LaunchCiv5();
        }

        private void ModButton_Clicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ModModel mod = button.DataContext as ModModel;
            if(mod.IsDownloaded)
            {
                HelperFunctions.UpdateToMod(mod);
            } else
            {
                DownloadMod(mod);
            }
        }

        private void DownloadMod(ModModel mod)
        {
            if (progressPopupWindow != null)
            {
                return;
            }
            progressPopupWindow = new ProgressPopupWindow();
            progressPopupWindow.ProgressBar.IsIndeterminate = false;
            progressPopupWindow.Owner = Application.Current.MainWindow;
            progressPopupWindow.Show();

            string modsFolder = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\mods\";
            if (!Directory.Exists(modsFolder))
            {
                Directory.CreateDirectory(modsFolder);
            }
            string modFolder = modsFolder + mod.Name;
            if (!Directory.Exists(modFolder))
            {
                Directory.CreateDirectory(modFolder);
            }
            
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFileCompleted += DownloadCompleted;
                wc.DownloadProgressChanged += UpdatePopupProgressBar;
                wc.QueryString.Add("modFolder", modFolder);
                wc.DownloadFileAsync(new System.Uri(mod.DownloadURL), @"mods\" + mod.Name + @"\mod.zip");
            }
        }
        
        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string modFolder = ((System.Net.WebClient)(sender)).QueryString["modFolder"];
                if (e.Cancelled || e.Error != null)
                {
                    if (Directory.Exists(modFolder))
                    {
                        Directory.Delete(modFolder, true);
                    }

                    ErrorPopupWindow errorPopup = new ErrorPopupWindow();
                    errorPopup.textBox.Text = e.Error.ToString();
                    errorPopup.Owner = Application.Current.MainWindow;
                    errorPopup.Show();
                }
                else
                {
                    ZipFile.ExtractToDirectory(modFolder + @"\mod.zip", modFolder);
                    File.Delete(modFolder + @"\mod.zip");
                    UpdateModListDownloadedStatus();
                }
            }
            finally
            {
                if (progressPopupWindow != null)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => { progressPopupWindow.Close(); }));
                    progressPopupWindow = null;
                }
            }
        }

        private void UpdateModsList(object sender, EventArgs e)
        {
            if (progressPopupWindow != null)
            {
                return;
            }
            progressPopupWindow = new ProgressPopupWindow();
            progressPopupWindow.ProgressBar.IsIndeterminate = true;
            progressPopupWindow.Owner = Application.Current.MainWindow;
            progressPopupWindow.Show();
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += DoUpdateModsList;
            worker.ProgressChanged += UpdatePopupProgressBar;
            worker.RunWorkerAsync();
        }

        private void UpdateModListDownloadedStatus()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                foreach (ModModel mod in modModels)
                {
                    mod.IsDownloaded = HelperFunctions.IsModDownloaded(mod.Name);
                    mod.IsInstalled = HelperFunctions.IsModInstalled(mod.Name);
                }
            }));
        }

        private void UpdatePopupProgressBar(object sender, ProgressChangedEventArgs e)
        {
            if (progressPopupWindow != null)
            {
                progressPopupWindow.ProgressBar.Value = e.ProgressPercentage;
            }
        }

        private void DoUpdateModsList(object sender, DoWorkEventArgs e)
        {
            try
            {
                ModList modList = HelperFunctions.GetModsListFromServer();
                if(modList.latestClientVersion != Globals.VERSION)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        NewVersionPopupWindow popup = new NewVersionPopupWindow(modList.latestClientDownloadUrl);
                        popup.Owner = Application.Current.MainWindow;
                        popup.Show();
                    }));
                    return;
                }

                Application.Current.Dispatcher.Invoke(new Action(() => { modModels.Clear(); }));
            
                foreach (ModInfo modInfo in modList.mods)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() => 
                    {
                        modModels.Add(new ModModel()
                        {
                            Name = modInfo.Name,
                            Released = modInfo.Released.ToString("yyyy-MM-dd"),
                            IsDownloaded = HelperFunctions.IsModDownloaded(modInfo.Name),
                            IsInstalled = HelperFunctions.IsModInstalled(modInfo.Name),
                            DownloadURL = modInfo.DownloadURL
                        });
                    }));
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ErrorPopupWindow errorPopup = new ErrorPopupWindow();
                    errorPopup.textBox.Text = ex.ToString();
                    errorPopup.Owner = Application.Current.MainWindow;
                    errorPopup.Show();
                }));
            }
            finally
            {
                if(progressPopupWindow != null)
                {
                    Thread.Sleep(300); // Chill a bit to allow the loading-bar to be visible if the request completed really quickly to show the user that something actually happened when the refresh button was pressed.
                    Application.Current.Dispatcher.Invoke(new Action(() => { progressPopupWindow.Close(); }));
                    progressPopupWindow = null;
                }
            }
        }
    }
}
