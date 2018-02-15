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
using static MaloWLauncher.ModModel;

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

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(ModsList.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Name");
            view.GroupDescriptions.Add(groupDescription);
        }

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (!HelperFunctions.IsConfigGameLocationValid())
            {
                SetGameLocationPopupWindow popup = new SetGameLocationPopupWindow();
                popup.Owner = Application.Current.MainWindow;
                popup.Show();
            }
            UpdateModsList(sender, e);
            this.VersionLabel.Content = "v" + Globals.VERSION;
        }

        private void ListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // A hackfix to be able to have content right-aligned in the last column of the grid
            ListView listView = sender as ListView;
            GridView gView = listView.View as GridView;

            var workingWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;
            var col1 = 0.25;
            var col2 = 0.12;
            var col3 = 0.23;
            var col4 = 0.45;

            gView.Columns[0].Width = workingWidth * col1;
            gView.Columns[1].Width = workingWidth * col2;
            gView.Columns[2].Width = workingWidth * col3;
            gView.Columns[3].Width = workingWidth * col4;
        }

        private void Refresh_Clicked(object sender, RoutedEventArgs e)
        {
            UpdateModsList(sender, e);
        }

        private void Options_Clicked(object sender, RoutedEventArgs e)
        {
            OptionsPopupWindow popup = new OptionsPopupWindow();
            popup.Owner = Application.Current.MainWindow;
            popup.Show();
        }

        private void UninstallAllMods_Clicked(object sender, RoutedEventArgs e)
        {
            HelperFunctions.InstallMod(null);
            UpdateModsList(sender, e);
        }

        private void LaunchCiv5_Clicked(object sender, RoutedEventArgs e)
        {
            HelperFunctions.LaunchCiv5();
        }

        private void DownloadMod_Clicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ModModel mod = button.DataContext as ModModel;
            DownloadMod(mod);
            UpdateModListStatuses();
        }

        private void InstallMod_Clicked(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            ModModel mod = button.DataContext as ModModel;
            HelperFunctions.InstallMod(mod);
            UpdateModListStatuses();
        }

        private void GroupingExpander_Clicked(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            string modName = (string) expander.Tag;
            if(expander.IsExpanded)
            {
                HelperFunctions.AddExpandedModToConfig(modName);
            }
            else
            {
                HelperFunctions.RemoveExpandedModToConfig(modName);
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
            string modFolder = modsFolder + mod.GetFullName();
            if (!Directory.Exists(modFolder))
            {
                Directory.CreateDirectory(modFolder);
            }
            
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFileCompleted += DownloadCompleted;
                wc.DownloadProgressChanged += UpdatePopupProgressBar;
                wc.QueryString.Add("modFolder", modFolder);
                wc.DownloadFileAsync(new System.Uri(mod.DownloadURL), @"mods\" + mod.GetFullName() + @"\mod.zip");
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
            worker.DoWork += UpdateModsList;
            worker.ProgressChanged += UpdatePopupProgressBar;
            worker.RunWorkerAsync();
        }

        private void UpdateModListStatuses()
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                foreach (ModModel mod in modModels)
                {
                    mod.IsDownloaded = HelperFunctions.IsModDownloaded(mod.GetFullName());
                    mod.IsInstalled = HelperFunctions.IsModInstalled(mod.GetFullName());
                    mod.IsExpanded = HelperFunctions.IsModExpanded(mod.Name);
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
        
        private void UpdateModsList(object sender, DoWorkEventArgs e)
        {
            try
            {
                ModList modList = HelperFunctions.GetModsListFromServer();
                if(HelperFunctions.IsNewVersionRequired(modList.latestClientVersion))
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        NewVersionPopupWindow popup = new NewVersionPopupWindow(modList.latestClientDownloadUrl);
                        popup.Owner = Application.Current.MainWindow;
                        popup.ShowDialog();
                    }));
                }

                Application.Current.Dispatcher.Invoke(new Action(() => { modModels.Clear(); }));
            
                foreach (Mod mod in modList.mods)
                {
                    foreach (ModList.Mod.Version version in mod.versions)
                    {
                        Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            modModels.Add(new ModModel()
                            {
                                Name = mod.name,
                                Released = version.released.ToString("yyyy-MM-dd"),
                                Version = version.version,
                                DownloadURL = version.downloadURL
                            });
                        }));
                    }
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
                    UpdateModListStatuses();
                    Thread.Sleep(300); // Chill a bit to allow the loading-bar to be visible if the request completed really quickly to show the user that something actually happened when the refresh button was pressed.
                    Application.Current.Dispatcher.Invoke(new Action(() => { progressPopupWindow.Close(); }));
                    progressPopupWindow = null;
                }
            }
        }
    }
}
