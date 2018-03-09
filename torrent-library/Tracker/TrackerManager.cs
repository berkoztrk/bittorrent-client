using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Model;

namespace torrent_library.Tracker
{
    public static class TrackerManager
    {
        public static async Task ConnectToTrackers(Torrent torrent, TorrentManager torrentManager)
        {
            await Task.Factory.StartNew(() =>
            {
                foreach (var t in torrent.Trackers.Where(x => x[0].StartsWith("udp")).Select(x => x[0]))
                {
                    var ta = new TrackerAdress(t);
                    var tracker = new Tracker(ta, torrent, torrentManager);
                    var task = Task.Run(() =>
                    {
                        try
                        {
                            tracker.ConnectToTracker();
                        }
                        catch (Exception e)
                        {
                        }
                    }).ContinueWith(x =>
                    {
                        if (tracker.IsConnected)
                        {
                            tracker.Scrape();
                            tracker.Announce();
                        }
                    });
                }
            });

        }
    }
}
