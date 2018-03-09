using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Model
{
    public class TorrentWithTrackerInfo
    {
        public Torrent _Torrent { get; set; }
        public List<TrackerAdress> TrackerAddresses { get; set; }

        public TorrentWithTrackerInfo() { }

        public TorrentWithTrackerInfo(Torrent torrent, List<TrackerAdress> trackerAddress)
        {
            _Torrent = torrent;
            TrackerAddresses = trackerAddress;
        }
    }
}
