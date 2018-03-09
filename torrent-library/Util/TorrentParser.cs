using BencodeNET.Parsing;
using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.MagnetUtils;
using torrent_library.Model;

namespace torrent_library.Util
{
    public static class TorrentParser
    {
        public static TorrentWithTrackerInfo ParseTorrent(string magnetURI)
        {
            var magnetURIObj = new MagnetURI(magnetURI);
            var magnetToTorrent = new MagnetToTorrent(magnetURIObj.MagnetDefinition.InfoHash);
            var path = magnetToTorrent.Download();
            var parser = new BencodeParser();
            var torrent = parser.Parse<Torrent>(path);
            return new TorrentWithTrackerInfo(torrent, magnetURIObj.MagnetDefinition.tr);
        }
    }
}
