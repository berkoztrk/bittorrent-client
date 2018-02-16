using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Model
{
    public static class AnnounceAction
    {
        public static readonly int None = 0;
        public static readonly int Completed = 1;
        public static readonly int Started = 2;
        public static readonly int Stopped = 3;
    }
}
