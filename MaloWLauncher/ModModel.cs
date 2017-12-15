using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MaloWLauncher
{
    class ModModel : INotifyPropertyChanged
    {
        private string NameValue = String.Empty;
        public string Released { get; set; }
        public string ButtonText { get; set; } = "Download";
        private bool IsDownloadedValue = false;
        private bool IsInstalledValue = false;
        public string InstalledText { get; set; } = "";
        public string DownloadURL { get; set; }

        public string Name
        {
            get
            {
                return this.NameValue;
            }

            set
            {
                if (value != this.NameValue)
                {
                    this.NameValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsDownloaded
        {
            get
            {
                return this.IsDownloadedValue;
            }

            set
            {
                if (value != this.IsDownloadedValue)
                {
                    this.IsDownloadedValue = value;
                    if (this.IsDownloadedValue == true)
                    {
                        this.ButtonText = "Install";
                    }
                    else
                    {
                        this.ButtonText = "Download";
                    }
                    NotifyPropertyChanged("ButtonText");
                }
            }
        }

        public bool IsInstalled
        {
            get
            {
                return this.IsInstalledValue;
            }

            set
            {
                if (value != this.IsInstalledValue)
                {
                    this.IsInstalledValue = value;
                    if (this.IsInstalledValue == true)
                    {
                        this.InstalledText = "";
                    }
                    else
                    {
                        this.InstalledText = "(INSTALLED)";
                    }
                    NotifyPropertyChanged("CanInstall");
                }
            }
        }

        public bool CanInstall
        {
            get
            {
                return !this.IsInstalledValue;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
