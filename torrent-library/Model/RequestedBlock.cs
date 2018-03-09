using System;
using System.Linq;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class RequestedBlock
    {

        public int Piece { get; set; }
        public int Block { get; set; }
        public int Length { get; set; }
        public byte[] Data { get; internal set; }

        public RequestedBlock(int piece, int block, int length)
        {
            Piece = piece;
            Block = block;
            Length = length;
        }

        public override string ToString()
        {
            return string.Join("_", Piece, Block, Length);
        }



        public static RequestedBlock GetBlock(Peer peer, TorrentManager manager)
        {
            RequestedBlock block = null;
            for (int i = 0; i < peer.Bitfield.Length; i++)
            {
                for (int j = 0; j < manager.DownloadProgress[i].Length; j++)
                {
                    if (manager.DownloadProgress[i][j])
                        continue;
                    block = new RequestedBlock(i, j, TorrentPieceUtil.GetBlockSize(i, j, manager.Torrent));
                    return block;
                }
            }
            return block;
        }
    }
}