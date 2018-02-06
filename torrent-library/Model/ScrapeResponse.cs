using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Model
{
    public class ScrapeResponse
    {
        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public int Completed { get; set; }
    }
}
