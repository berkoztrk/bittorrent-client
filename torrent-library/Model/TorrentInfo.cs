using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Model
{
    public class TorrentInfo
    {
        public int TotalSeeders { get; set; }
        public int ConnectedSeeders { get; set; }

        public TorrentInfo()
        {
            TotalSeeders = 0;
            ConnectedSeeders = 0;
        }
    }
}
