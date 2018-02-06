using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using torrent_library;
using torrent_library.Tracker;

namespace torrent_library_test
{
    class Program
    {
        private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:68e9c21729fadbd19b8933951fcc7ed2ab3e2b31&dn=Amelie+%282001%29+720p+BRrip_sujaidr&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";

        static void Main(string[] args)
        {
            var magnetURIObj = new MagnetURI(TEST_MAGNET_URI);
            var t = magnetURIObj.MagnetDefinition.tr[0];
            var tracker = new UDPTracker(t);
            tracker.Connect();


            Console.Read();
        }
    }
}
