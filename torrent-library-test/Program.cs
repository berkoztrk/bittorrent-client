using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using torrent_library;
using torrent_library.Downloader;
using torrent_library.MagnetUtils;
using torrent_library.Model;
using torrent_library.Tracker;
using torrent_library.Util;

namespace torrent_library_test
{
    class Program
    {

        //private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:86e8a680f40c18e8fde284e99f235764ff59cf91&dn=Maluma+Ft.+Jenny+La+Sexy+Voz+-+Intentalo+%28Remix%29+-+1080p&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        private const string DESPACITO = "magnet:?xt=urn:btih:7fbc58e324b539bdda58c15bda3acd26b0d5fbd1&dn=Luis+Fonsi+-+Despacito+%28feat.+Daddy+Yankee%29&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        private const string BBT = "magnet:?xt=urn:btih:a53e3226a59f4032eefa26cefe6a9bdee17aa970&dn=KMSpico+10.1.8+FINAL+%2B+Portable+%28Office+and+Windows+10+Activator&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        private const string AMELIE = "magnet:?xt=urn:btih:d1c5bd9a2eec5fe6eba19e7f663af3e8d932ab8e&dn=Amelie+%5BAm%C3%83%C2%A9lie+Poulain%5D.2001.BRRip.x264.AAC%5B5.1%5D-VLiS&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        //private const string TEST_MAGNET_URI = "magnet:?xt=urn:btih:01c227c8c9aac311f9365b163ea94708c27a7db4&dn=The+Subtle+Art+of+Not+Giving+a+Fck+%282016%29+%28Epub%29+Gooner&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969&tr=udp%3A%2F%2Fzer0day.ch%3A1337&tr=udp%3A%2F%2Fopen.demonii.com%3A1337&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969&tr=udp%3A%2F%2Fexodus.desync.com%3A6969";
        private const string TEST_INFO_HASH = "2b90241f8e95d53cacf0f8c72a92e46b57911600";
        private static TorrentProcessor processor;

        static void Main(string[] args)
        {

            processor = new TorrentProcessor();
            processor.StartProcess(AMELIE);

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
            }

            processor.Manager.SaveAsJSON();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            processor.Manager.SaveAsJSON();
        }

    }

}


