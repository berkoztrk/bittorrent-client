using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library
{
    public class MagnetURIDefinition
    {
        /// <summary>
        /// (Display Name) – Filename
        /// </summary>
        public string dn { get; set; }
        /// <summary>
        /// xt (eXact Topic) – URN containing file hash
        /// </summary>
        public string xt { get; set; }
        /// <summary>
        /// tr (address TRacker) – Tracker URL for BitTorrent downloads
        /// </summary>
        public List<TrackerAdress> tr { get; set; }
        /// <summary>
        /// InfoHash
        /// </summary>
        public string InfoHash { get; set; }

        public MagnetURIDefinition()
        {
            tr = new List<TrackerAdress>();
        }
    }
}
