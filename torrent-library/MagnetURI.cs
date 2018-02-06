using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library
{
    public class MagnetURI
    {
        private string _MagnetURI { get; set; }
        public MagnetURIDefinition MagnetDefinition { get; set; }

        public MagnetURI(string magnetURI)
        {
            _MagnetURI = magnetURI;
            MagnetDefinition = MagnetURIParser.Parse(magnetURI);
        }

    }
}
