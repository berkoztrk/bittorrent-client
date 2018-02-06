using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library
{
    public class TrackerAdress
    {
        public string FullAddress { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public TrackerAdress(string address)
        {
            this.FullAddress = address;
            this.Host = address.Slice(address.IndexOfNth("/", 2) + 1, address.IndexOfNth(":", 2));
            this.Port = int.Parse(address.Substring(address.IndexOfNth(":", 2) + 1));
        }
    }
}
