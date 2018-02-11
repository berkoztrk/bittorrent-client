using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class AnnounceResponse
    {
        public int Action { get; set; }
        public int TransactionID { get; set; }
        public int Interval { get; set; }
        public int Leechers { get; set; }
        public int Seeders { get; set; }
        public int IPAddress { get; set; }
        public short Port { get; set; }

        public List<int> IPAddresses { get; set; }
        public List<ushort> Ports
        {
            get; set;
        }

        public AnnounceResponse() { }

        public AnnounceResponse(byte[] announceResponse)
        {

            //            0           32 - bit integer action          1 // announce
            //4           32 - bit integer transaction_id
            //8           32 - bit integer interval
            //12          32 - bit integer leechers
            //16          32 - bit integer seeders
            //20 + 6 * n  32 - bit integer IP address
            //24 + 6 * n  16 - bit integer TCP port
            //20 + 6 * N

            IPAddresses = new List<int>();
            Ports = new List<ushort>();

            int n = ((announceResponse.Length - 20) / 6) - 1;
            Action = BitConverterUtil.ToInt(announceResponse.SubArray(0, 4));
            TransactionID = BitConverterUtil.ToInt(announceResponse.SubArray(4, 4));
            Interval = BitConverterUtil.ToInt(announceResponse.SubArray(8, 4));
            Leechers = BitConverterUtil.ToInt(announceResponse.SubArray(12, 4));
            Seeders = BitConverterUtil.ToInt(announceResponse.SubArray(16, 4));

            for (int i = 0; i < n; i++)
            {
                var ipAddress = BitConverterUtil.ToInt(announceResponse.SubArray(20 + 6 * i, 4));
                var port = BitConverterUtil.ToShortInt(announceResponse.SubArray(24 + 6 * i, 2));

                IPAddresses.Add(ipAddress);
                Ports.Add(port);
            }
            //IPAddress = BitConverterUtil.ToInt(announceResponse.SubArray(20, 4));
            //Port = BitConverterUtil.ToShortInt(announceResponse.SubArray(24, 4));

            //IPString = BitConverterUtil.IntToIP(IPAddress);
            //PortString = Port.ToString();

        }

    }
}
