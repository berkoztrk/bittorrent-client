using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
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
            return string.Join("_", Piece, Block);
        }

        public static RequestedBlock GetBlock(Peer peer, TorrentManager manager, DownloadStrategy strategy, List<int> rarestPieces)
        {
            RequestedBlock block = null;

            if (strategy == DownloadStrategy.RarestFirst)
            {
                GetBlockByRarestFirst(out block, peer, manager, rarestPieces);
            }
            else
            {
                GetBlockOrdered(out block, peer, manager);
            }

            return block;
        }

        private static void GetBlockByRarestFirst(out RequestedBlock block, Peer peer, TorrentManager manager, List<int> rarestPieces)
        {
            for (int i = 0; i < rarestPieces.Count; i++)
            {
                int index = rarestPieces[i];
                if (!peer.Bitfield[index])
                    continue;

                for (int j = 0; j < manager.DownloadProgress[index].Length; j++)
                {
                    if (peer.IsBlockRequested[index][j] || manager.DownloadProgress[index][j])
                        continue;

                    block = new RequestedBlock(index, j, TorrentPieceUtil.GetBlockSize(i, j, manager.Torrent));
                    return;
                }
            }
            block = null;
        }

        private static void GetBlockOrdered(out RequestedBlock block, Peer peer, TorrentManager manager)
        {

            for (int i = 0; i < peer.Bitfield.Length; i++)
            {
                if (!peer.Bitfield[i])
                    continue;

                for (int j = 0; j < manager.DownloadProgress[i].Length; j++)
                {
                    if (peer.IsBlockRequested[i][j] || manager.DownloadProgress[i][j])
                        continue;
                    block = new RequestedBlock(i, j, TorrentPieceUtil.GetBlockSize(i, j, manager.Torrent));
                    return;
                }
            }
            block = null;

        }

        public static List<int> GetOrderedPiecesByRarest(TorrentManager manager)
        {
            Dictionary<int, int> list = new Dictionary<int, int>();

            foreach (var kv in manager.Peers)
            {
                var peer = kv.Value;

                for (int i = 0; i < peer.Bitfield.Length; i++)
                {
                    if (peer.Bitfield[i])
                    {
                        if (list.ContainsKey(i))
                            list[i]++;
                        else
                            list[i] = 1;
                    }
                }
            }

            return list.OrderBy(x => x.Value).Select(x => x.Key).ToList();
        }

        public List<FileDownloadInfo> GetFilePieceBelongs(TorrentManager manager)
        {
            long pieceSize = TorrentPieceUtil.GetPieceSize(Piece, manager.Torrent);
            long currentPieceOffset = Piece * TorrentPieceUtil.GetPieceSize(Piece > 0 ? Piece - 1 : Piece, manager.Torrent);
            List<FileDownloadInfo> files = new List<FileDownloadInfo>();

            if (manager.Torrent.FileMode == TorrentFileMode.Single)
            {
                files.Add(new FileDownloadInfo()
                {
                    FileInfo = new MultiFileInfo()
                    {
                        FileSize = manager.Torrent.File.FileSize,
                        Path = new List<string>() { manager.Torrent.File.FileName },
                    },
                    Length = TorrentPieceUtil.GetPieceSize(Piece, manager.Torrent),
                    FileOffset = currentPieceOffset
                });
            }
            else
            {
                foreach (var file in manager.Torrent.Files)
                {
                    var fileSize = file.FileSize;

                    var diff = fileSize;
                    if (currentPieceOffset + pieceSize <= fileSize)
                    {
                        files.Add(new FileDownloadInfo()
                        {
                            FileInfo = file,
                            FileOffset = currentPieceOffset,
                            Length = pieceSize
                        });
                        pieceSize = 0;
                    }
                    else if (currentPieceOffset + pieceSize >= fileSize && currentPieceOffset < fileSize)
                    {
                        files.Add(new FileDownloadInfo()
                        {
                            FileInfo = file,
                            FileOffset = currentPieceOffset,
                            Length = fileSize - currentPieceOffset
                        });
                        diff = currentPieceOffset;
                        pieceSize -= fileSize - currentPieceOffset;
                    }

                    if (pieceSize == 0)
                        break;

                    currentPieceOffset -= fileSize;

                    if (currentPieceOffset < 0)
                        currentPieceOffset = 0;

                }
            }
            return files;
        }
    }
}