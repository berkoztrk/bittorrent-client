using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bittorrent_client.Model
{
    public class TorrentModel
    {
        public string Name { get; set; }
        public int Progress { get; set; }
        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public int DLSpeed { get; set; }
        public int ULSpeed { get; set; }
    }
}
