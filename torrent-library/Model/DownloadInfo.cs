using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Model
{
    public class DownloadInfo
    {
        public byte[] PeerID { get; set; }
        public long Downloaded { get; set; }
        public bool[] Bitfield { get; set; }
        public Dictionary<int, PieceInfo> PieceInfos { get; set; }
        public bool[][] DownloadProgress { get; internal set; }

        public DownloadInfo()
        {

        }
    }
}
