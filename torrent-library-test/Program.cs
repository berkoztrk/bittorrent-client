using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using torrent_library;
using torrent_library.Downloader;
using torrent_library.Tracker;

namespace torrent_library_test
{
    class Program
    {
        //private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:68e9c21729fadbd19b8933951fcc7ed2ab3e2b31&dn=Amelie+%282001%29+720p+BRrip_sujaidr&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:d1c5bd9a2eec5fe6eba19e7f663af3e8d932ab8e&dn=Amelie+%5BAm%C3%83%C2%A9lie+Poulain%5D.2001.BRRip.x264.AAC%5B5.1%5D-VLiS&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        private const string TEST_INFO_HASH = "2b90241f8e95d53cacf0f8c72a92e46b57911600";


        static void Main(string[] args)
        {
            var magnetURIObj = new MagnetURI(TEST_MAGNET_URI);
            //var t = magnetURIObj.MagnetDefinition.tr[0];
            //var tracker = new UDPTracker(t);
            //tracker.ConnectToTracker();

            UDPTracker tracker = null;
            foreach (var t in magnetURIObj.MagnetDefinition.tr)
            {
                tracker = new UDPTracker(t, magnetURIObj.MagnetDefinition.InfoHash);
                tracker.Scrape();

                if (tracker.IsConnected)
                    break;
            }

            if(tracker != null)
            {
                
                while (true)
                {
                    var torrentDownloader = new TorrentDownloader(tracker._AnnounceResponse);
                    var bytesDownloaded = torrentDownloader.Download();

                    if(bytesDownloaded == 0 && ((DateTime.Now  - tracker.LastAnnounced).TotalSeconds > tracker._AnnounceResponse.Interval))
                    {
                        Console.WriteLine((DateTime.Now - tracker.LastAnnounced).TotalSeconds);
                        Console.WriteLine(tracker._AnnounceResponse.Interval);
                        tracker.Scrape();
                    }

                    Thread.Sleep(500);
                }
            }
        }
    }
}
