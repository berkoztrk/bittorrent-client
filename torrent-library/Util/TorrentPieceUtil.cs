using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Model;

namespace torrent_library.Util
{
    public static class TorrentPieceUtil
    {
        public const int CHUNK_SIZE = 16384;

        public static int GetBlockCount(int piece, Torrent torrent)
        {
            return Convert.ToInt32(Math.Ceiling(GetPieceSize(piece, torrent) / (double)CHUNK_SIZE));
        }

        public static int GetPieceSize(int piece, Torrent torrent)
        {
            if (piece == torrent.NumberOfPieces - 1)
            {
                int remainder = Convert.ToInt32(torrent.TotalSize % torrent.PieceSize);
                if (remainder != 0)
                    return remainder;
            }

            return (int)torrent.PieceSize;
        }

        public static int GetBlockSize(int piece, int block, Torrent torrent)
        {
            if (block == GetBlockCount(piece, torrent) - 1)
            {
                int remainder = Convert.ToInt32(GetPieceSize(piece, torrent) % CHUNK_SIZE);
                if (remainder != 0)
                    return remainder;
            }

            return CHUNK_SIZE;
        }


        public static long GetFileOffset(RequestedBlock e, Torrent torrent)
        {
            return (e.Piece * torrent.PieceSize) + (e.Block * CHUNK_SIZE);
        }
    }
}
