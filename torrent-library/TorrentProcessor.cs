using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using torrent_library.Downloader;
using torrent_library.Model;
using torrent_library.Tracker;
using torrent_library.Util;

namespace torrent_library
{
    public class TorrentProcessor
    {
        public TorrentManager Manager { get; set; }
        public TorrentDownloader Downloader { get; set; }

        public TorrentProcessor()
        {

        }

        public void StartProcess(string magnetURI)
        {
            var torrent = TorrentParser.ParseTorrent(magnetURI);
            TorrentManager torrentManager = TorrentManager.Create(torrent._Torrent.OriginalInfoHash, torrent);

            Manager = torrentManager;

            TrackerManager.ConnectToTrackers(torrent._Torrent, torrentManager);


            var torrentDownloader = new TorrentDownloader(torrentManager);
            Downloader = torrentDownloader;
            new Thread(new ThreadStart(() =>
            {
                torrentDownloader.StartDownload();
            })).Start();



        }

    }
}
