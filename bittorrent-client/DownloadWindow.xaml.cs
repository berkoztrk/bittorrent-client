using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using torrent_library;
using System.Linq;
using torrent_library.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using bittorrent_client.Model;
using System;
using bittorrent_client.BO;
using System.Windows;
using System.ComponentModel;

namespace bittorrent_client
{

    public partial class DownloadWindow : Page
    {
        public SqTorrentContext Context;
        public ObservableCollection<TorrentItem> DownloadsDataSource = new ObservableCollection<TorrentItem>();
        private readonly BackgroundWorker worker = new BackgroundWorker() { WorkerSupportsCancellation = true };

        public DownloadWindow()
        {
            InitializeComponent();
            worker.DoWork += Worker_DoWork;
            Context = SqTorrentContext.GetInstance();
            Context.ProcessorsUpdated += Context_ProcessorsUpdated;
            Init();
            worker.RunWorkerAsync();
            DownloadsDataSource.CollectionChanged += DownloadsDataSource_CollectionChanged;
        }

        private void Context_ProcessorsUpdated(object arg1, TorrentProcessor arg2)
        {
            var ds = (ObservableCollection<TorrentItem>)DataGridDownloads.ItemsSource;

            foreach (var processor in Context.Processors)
            {
                if (ds.Count(x => x.InfoHash == processor.Key) == 0)
                    ds.AddOnUI(new TorrentItem(processor.Value.Manager.Torrent.DisplayName, "0", 0, processor.Key));
            }
        }

        private void DownloadsDataSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DataGridDownloads.Items.Refresh();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!worker.CancellationPending)
            {
                try
                {
                    foreach (var kv in Context.Processors)
                    {
                        var processor = kv.Value;
                        var manager = processor.Manager;

                        var item = ((ObservableCollection<TorrentItem>)DataGridDownloads.ItemsSource).Where(x => x.InfoHash == manager.Torrent.OriginalInfoHash).FirstOrDefault();
                        if (item != null)
                        {
                            item.Peers = string.Format("{0}/{1}", manager.Peers.Count(x => !x.Value.IsDisconnected), manager.Peers.Count);
                            item.Progress = (manager.Downloaded * 100 / manager.Torrent.TotalSize);
                            item.DownloadSpeed = Math.Ceiling((double)manager.DownloadedForSpeed) + "kb/s";
                        }

                    }
                }
                catch (Exception ex)
                {

                }

            }
        }

        public void Init()
        {
            DataGridDownloads.ItemsSource = DownloadsDataSource;
        }



    }
}
