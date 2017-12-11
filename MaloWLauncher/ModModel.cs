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
                        this.ButtonText = "Play";
                    }
                    else
                    {
                        this.ButtonText = "Download";
                    }
                    NotifyPropertyChanged("ButtonText");
                }
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
