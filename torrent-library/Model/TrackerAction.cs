using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Model
{
    public class TrackerAction
    {

        public static readonly int Connect = 0;
        public static readonly int Announce = 1;
        public static readonly int Scrape = 2;
        public static readonly int Error = 3;

    }
}
