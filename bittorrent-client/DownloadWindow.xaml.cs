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

        private void Context_ProcessorsUpdated(object arg1, TorrentProcessor processor)
        {
            var ds = (ObservableCollection<TorrentItem>)DataGridDownloads.ItemsSource;
            ds.AddOnUI(new TorrentItem(processor.Manager.Torrent.DisplayName, "0", 0, processor.Manager.Torrent.OriginalInfoHash));
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
                            var _progress = Math.Ceiling((double)(manager.Downloaded * 100 / manager.Torrent.TotalSize));
                            item.Peers = string.Format("{0}/{1}", manager.Peers.Count(x => !x.Value.IsDisconnected), manager.Peers.Count);
                            item.Progress = _progress > 100 ? 100 : _progress;
                            item.DownloadSpeed = Util.GetDownloadSpeed(processor.Downloader.DLSpeed);
                            item.TorrentStatus = manager.GetStatusText();
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
            if (AppConfig.TestMode)
            {
                var magnets = new string[]
{               "magnet:?xt=urn:btih:0b5e2de379a26bc298b16427fc328af38c4912d9&dn=J.+Cole+-+KOD+%282018%29+Mp3+%28320kbps%29+%5BHunter%5D&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969",
                "magnet:?xt=urn:btih:7fbc58e324b539bdda58c15bda3acd26b0d5fbd1&dn=Luis+Fonsi+-+Despacito+%28feat.+Daddy+Yankee%29&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969"
};

                foreach (var magn in magnets)
                {
                    TorrentProcessor processor = new TorrentProcessor();
                    processor.StartProcess(magn);
                    Context.AddProcessor(processor);
                }
            }
        }

        private void DataGridDownloads_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }


        private void StartDownload_Click(object sender, RoutedEventArgs e)
        {
            var item = DataGridDownloads.SelectedItem as TorrentItem;
            var downloader = Context.Processors[item.InfoHash].Downloader;
            if (downloader.Paused)
            {
                downloader.ContinueDownload();
                (sender as MenuItem).Header = "Pause";
            }
            else
            {
                (sender as MenuItem).Header = "Start";
                downloader.PauseDownload();
            }
        }

        private void RemoveTorrent_Click(object sender, RoutedEventArgs e)
        {
            var item = DataGridDownloads.SelectedItem as TorrentItem;
            Context.Processors[item.InfoHash].Manager.RemoveTorrent();
            TorrentProcessor proc = null;
            Context.Processors.TryRemove(item.InfoHash, out proc);
            DownloadsDataSource.Remove(item);
            DataGridDownloads.ItemsSource = DownloadsDataSource;

        }
    }
}
