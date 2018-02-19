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
        private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:555c6502327eddbf41f268690d4f97cb8d756372&dn=Penthouse+Magazine+July+and+August+2017++The+Erotic+Fetish+Issue&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        //private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:d1c5bd9a2eec5fe6eba19e7f663af3e8d932ab8e&dn=Amelie+%5BAm%C3%83%C2%A9lie+Poulain%5D.2001.BRRip.x264.AAC%5B5.1%5D-VLiS&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        //private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:01c227c8c9aac311f9365b163ea94708c27a7db4&dn=The+Subtle+Art+of+Not+Giving+a+Fck+%282016%29+%28Epub%29+Gooner&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        private const string TEST_INFO_HASH = "2b90241f8e95d53cacf0f8c72a92e46b57911600";

        static void Main(string[] args)
        {
            var magnetURIObj = new MagnetURI(TEST_MAGNET_URI);
            var magnetToTorrent = new MagnetToTorrent(magnetURIObj.MagnetDefinition.InfoHash);
            var path = magnetToTorrent.Download();
            var torrent = TorrentParser.ParseTorrent(path);
            List<Tracker> connectedTrackers = new List<Tracker>();
            TorrentManager torrentInfo = new TorrentManager(torrent);

            //foreach (var file in torrent.Files)
            //{
            //    var p = string.Format(@"C:\torrents\{0}.downloading", file.FileName);
            //    if (File.Exists(p))
            //        File.Delete(p);
            //    File.Create(p);
            //    torrentInfo.FilePaths.Add(p);
            //}

            foreach (var t in magnetURIObj.MagnetDefinition.tr)
            {
                var tracker = new Tracker(t, magnetURIObj.MagnetDefinition.InfoHash, torrent, torrentInfo);
                var task = Task.Run(() =>
                {
                    try
                    {
                        tracker.ConnectToTracker();
                    }
                    catch { }

                }).ContinueWith(x =>
                {
                    if (tracker.IsConnected)
                    {
                        tracker.Scrape();
                        tracker.Announce();
                        connectedTrackers.Add(tracker);
                        
                        foreach(var ipport in tracker._AnnounceResponse.IPPort)
                        {
                            torrentInfo.NotConnectedPeers.Add(new Peer(ipport, torrent, torrentInfo));
                        }

                        //torrentInfo.NotConnectedPeers.ToList().AddRange(tracker._AnnounceResponse.IPPort.Select(y => new Peer(y, torrent, torrentInfo)).ToList());
                    }
                });
            }

            var torrentDownloader = new TorrentDownloader(torrent, torrentInfo);
            torrentDownloader.StartDownload();

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
            }

        }

    }

}


