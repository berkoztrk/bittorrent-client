using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public static class PeerIDUtil
    {
        private const string PREFIX = "-ST1000-";
        private static Random random = new Random();

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static byte[] GenerateRandom()
        {
            var peerID = PREFIX + RandomString(12);
            //var peerID = "-KS0001-123456654321";
            //var hash = SHA1Util.SHA1HashStringForUTF8String(peerID);K
            return BitConverterUtil.FromString(peerID);
        }
    }
}
