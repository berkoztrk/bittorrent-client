using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library
{
    public static class MagnetURIParser
    {
        public static MagnetURIDefinition Parse(string magnetURI)
        {
            var contents = Uri.UnescapeDataString(magnetURI).Replace("magnet:?", "").Split('&').ToList();
            var definition = new MagnetURIDefinition();

            foreach (var s in contents)
            {
                if (s.StartsWith("xt="))
                {
                    definition.xt = s.Replace("xt=", "");
                    definition.InfoHash = s.Replace("xt=urn:btih:", "");
                }
                else if (s.StartsWith("dn="))
                    definition.dn = s.Replace("dn=", "");
                else if (s.StartsWith("tr="))
                    definition.tr.Add(new TrackerAdress(s.Replace("tr=", "")));
            }


            return definition;

        }
    }
}
