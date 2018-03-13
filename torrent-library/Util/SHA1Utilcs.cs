using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BencodeNET.Torrents;
using torrent_library.Model;

namespace torrent_library.Util
{
    public static class SHA1Util
    {
        /// <summary>
        /// Compute hash for string encoded as UTF8
        /// </summary>
        /// <param name="s">String to be hashed</param>
        /// <returns>40-character hex string</returns>
        public static string SHA1HashStringForUTF8String(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);

            var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(bytes);

            return HexStringFromBytes(hashBytes);
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        private static string HexFromByteArray(byte[] bytes)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(bytes);
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        public static bool ValidatePiece(RequestedBlock block, Torrent torrent)
        {
            var len = torrent.PiecesAsHexString.Length / torrent.NumberOfPieces;
            var validHash = torrent.PiecesAsHexString.Substring(block.Piece * len, len);
            var ps = TorrentPieceUtil.GetPieceSize(0, torrent);
            var d = new byte[ps];
            Buffer.BlockCopy(block.Data, 0, d, 0, block.Data.Length);
            Buffer.BlockCopy(new byte[d.Length - block.Data.Length], 0, d, block.Data.Length, d.Length - block.Data.Length);
            var e = HexFromByteArray(d);
            if (validHash.ToUpperInvariant() == HexFromByteArray(block.Data).ToUpperInvariant())
                return true;
            else
                return false;
        }
    }
}
