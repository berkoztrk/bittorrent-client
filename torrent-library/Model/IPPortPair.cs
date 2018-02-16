using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class IPPortPair
    {
        public IPAddress IP { get; set; }
        public int Port { get; set; }

        public IPPortPair(int ip, int port)
        {
            this.Port = port;
            this.IP = IPAddress.Parse(BitConverterUtil.IntToIP(ip));
        }
    }
}
