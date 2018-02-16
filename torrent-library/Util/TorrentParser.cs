using BencodeNET.Parsing;
using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Util
{
    public static class TorrentParser
    {
        public static Torrent ParseTorrent(string path)
        {
            var parser = new BencodeParser();
            var torrent = parser.Parse<Torrent>(path);
            //var x = 5;
            return torrent;
        }
    }
}
