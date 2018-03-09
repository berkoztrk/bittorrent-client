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
        public int Progress { get; set; }

        public TorrentItem() { }

        public TorrentItem(string name, string peers, int progress)
        {
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
