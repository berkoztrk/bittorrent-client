using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Util
{
    public static class CryptoUtil
    {
        public static byte[] RandomPeerID()
        {
            var sha1 = SHA1.Create();
            return sha1.ComputeHash(new Guid().ToByteArray());
        }
    }
}
