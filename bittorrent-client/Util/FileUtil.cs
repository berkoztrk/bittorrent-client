using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bittorrent_client.Util
{
    public static class FileUtil
    {
        public static byte[] ConvertFileToByteArrayFromPath(string path)
        {
            return System.IO.File.ReadAllBytes(path);
        }

    }
}
