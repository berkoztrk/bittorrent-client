using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Controls;
using torrent_library;
using System.Linq;
using torrent_library.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using bittorrent_client.Model;

namespace bittorrent_client
{

    public partial class DownloadWindow : Page
    {

        public ObservableCollection<TorrentItem> DownloadsDataSource = new ObservableCollection<TorrentItem>();

        private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:555c6502327eddbf41f268690d4f97cb8d756372&dn=Penthouse+Magazine+July+and+August+2017++The+Erotic+Fetish+Issue&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        public DownloadWindow()
        {
            InitializeComponent();
            Init();

            Loaded += DownloadWindow_Loaded;
        }

        private void DownloadWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {

            var x = 5;
        }

        public void Init()
        {
            var torrentProcessor = new TorrentProcessor();
            torrentProcessor.StartProcess(TEST_MAGNET_URI);

            var item = new TorrentItem(torrentProcessor.Manager.Torrent.DisplayName, "0/0", 0);

            DownloadsDataSource.Add(item);
            DataGridDownloads.ItemsSource = DownloadsDataSource;

            new Thread(new ThreadStart(() =>
            {
                while (true)
                {

                    DownloadsDataSource[0].Peers = torrentProcessor.Manager.Peers.Count(x => x.Value.IsHandshakeReceived) + "/" + torrentProcessor.Manager.Peers.Count;
                    //Thread.Sleep(1000);
                }
            })).Start();

        }



    }
}
