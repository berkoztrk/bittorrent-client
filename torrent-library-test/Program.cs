using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using torrent_library;
using torrent_library.Downloader;
using torrent_library.MagnetUtils;
using torrent_library.Model;
using torrent_library.Tracker;
using torrent_library.Util;

namespace torrent_library_test
{
    class Program
    {
        private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:d1c5bd9a2eec5fe6eba19e7f663af3e8d932ab8e&dn=Amelie+%5BAm%C3%83%C2%A9lie+Poulain%5D.2001.BRRip.x264.AAC%5B5.1%5D-VLiS&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        private const string TEST_INFO_HASH = "2b90241f8e95d53cacf0f8c72a92e46b57911600";

        static void Main(string[] args)
        {
            var magnetURIObj = new MagnetURI(TEST_MAGNET_URI);
            var magnetToTorrent = new MagnetToTorrent(magnetURIObj.MagnetDefinition.InfoHash);
            var path = magnetToTorrent.DownloadTorrentFile();
            var torrent = TorrentParser.ParseTorrent(path);
            List<UDPTracker> connectedTrackers = new List<UDPTracker>();
            TorrentInfo torrentInfo = new TorrentInfo();

            foreach (var t in magnetURIObj.MagnetDefinition.tr)
            {
                var tracker = new UDPTracker(t, magnetURIObj.MagnetDefinition.InfoHash, torrent);
                var task = Task.Run(new Action(tracker.ConnectToTracker));
                task.GetAwaiter().OnCompleted(new Action(delegate ()
                {
                    if (tracker.IsConnected)
                    {
                        tracker.Scrape();
                        tracker.Announce();
                        connectedTrackers.Add(tracker);
                        var torrentDownloader = new TorrentDownloader(tracker._AnnounceRequest, tracker._AnnounceResponse, tracker._Torrent, torrentInfo);
                        torrentDownloader.StartDownload();
                    }
                }));
            }

            Console.Read();

            //while (true)
            //{
            //    //Thread.Sleep(1000);
            //    //ConsoleUtil.Write("Seeders => {0}/{1}", torrentInfo.ConnectedSeeders, torrentInfo.TotalSeeders);
            //    //ConsoleUtil.Write(trackerConnections.Where(x => x.Status == TaskStatus.RanToCompletion).Count().ToString());
            //    //Thread.Sleep(2000);
            //    //if (bestTracker != null)
            //    //{
            //    //    ConsoleUtil.Write(bestTracker._ScrapeResponse.Seeders.ToString());
            //    //}
            //    ////bestTracker.Announce();
            //}

        }

    }
}
