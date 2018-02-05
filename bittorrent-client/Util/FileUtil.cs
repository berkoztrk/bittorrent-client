using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bittorrent_client
{
    public static class FileUtil
    {
        public static byte[] ConvertFileToByteArrayFromPath(string path)
        {
            return File.ReadAllBytes(path);
        }

        public static string ConvertTorrentFileName(string fileName)
        {
            return Path.Combine(Util.AppSettings.TorrentFilePath + "\\" + fileName + ".torrent");
        }
    }
}
