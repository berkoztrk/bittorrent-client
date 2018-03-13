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
        public static void ConnectToTrackers(TorrentWithTrackerInfo twt, TorrentManager torrentManager)
        {
            Task.Factory.StartNew(() =>
           {
               foreach (var t in twt.TrackerAddresses)
               {
                   //var ta = new TrackerAdress(t);
                   var tracker = new Tracker(t, twt._Torrent, torrentManager);
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
