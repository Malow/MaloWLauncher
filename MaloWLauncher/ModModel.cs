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
        private string VersionValue = String.Empty;
        public string Released { get; set; }
        private bool IsDownloadedValue = false;
        private bool IsInstalledValue = false;
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

        public string Version
        {
            get
            {
                return this.VersionValue;
            }

            set
            {
                if (value != this.VersionValue)
                {
                    this.VersionValue = value;
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
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("IsNotDownloaded");
                    NotifyPropertyChanged("IsDownloadedButNotInstalled");
                }
            }
        }

        public bool IsNotDownloaded
        {
            get
            {
                return !this.IsDownloaded;
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
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("IsDownloadedButNotInstalled");
                }
            }
        }

        public bool IsDownloadedButNotInstalled
        {
            get
            {
                return this.IsDownloaded && !this.IsInstalled;
            }
        }

        private bool IsExpandedValue = false;
        public bool IsExpanded
        {
            get { return IsExpandedValue; }
            set
            {
                if (value != this.IsExpandedValue)
                {
                    IsExpandedValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string GetFullName()
        {
            return this.Name + " " + this.Version;
        }

        public override string ToString()
        {
            return this.GetFullName();
        }
    }
}
