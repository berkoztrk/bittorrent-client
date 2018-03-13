using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bittorrent_client.Model
{
    public class TorrentItem : INotifyPropertyChanged
    {
        private string peers;
        private double progress;
        private string downloadSpeed;

        public string Name { get; set; }
        public string Peers
        {
            get { return peers; }
            set
            {
                if (value != peers)
                {
                    peers = value;
                    OnPropertyChanged("Peers");
                }
            }
        }
        public string InfoHash { get; set; }
        public double Progress
        {
            get { return progress; }
            set
            {
                if (value != progress)
                {
                    progress = value;
                    OnPropertyChanged("Progress");
                }

            }
        }
        public string DownloadSpeed
        {
            get { return downloadSpeed; }
            set
            {
                if (value != downloadSpeed)
                {
                    downloadSpeed = value;
                    OnPropertyChanged("DownloadSpeed");
                }
            }
        }
        public TorrentItem() { }

        public TorrentItem(string name, string peers, double progress, string infoHash)
        {
            this.InfoHash = infoHash;
            this.Name = name;
            this.Peers = peers;
            this.Progress = progress;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

    }
}
