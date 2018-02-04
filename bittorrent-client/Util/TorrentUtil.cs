using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BENCODE = Bencode.BencodeUtility;
using MonoTorrent.Client;
using MonoTorrent;

namespace bittorrent_client.Util
{
    public static class TorrentUtil
    {
        public static Dictionary<string,object> Decode(string path)
        {
            return BENCODE.DecodeDictionary(FileUtil.ConvertFileToByteArrayFromPath(path));
        }

        public static void ConvertTorrentFileFromMagnetURI(string magnetURI)
        {
            //var manager = new TorrentManager(InfoHash.FromMagnetLink(magnetURI), Util.AppSettings.TargetPath, new TorrentSettings() {  });
        }
    }
}
