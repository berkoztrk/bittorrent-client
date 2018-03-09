using System;
using torrent_library.Model;
using torrent_library.Util;

namespace torrent_library
{
    internal class DownloadProcessor
    {
        TorrentManager Manager;
        TorrentFileWriter FileWriter;

        public DownloadProcessor()
        {
            Manager = TorrentManager.GetInstance();
            FileWriter = new TorrentFileWriter();
        }

        //public void Process()
        //{
        //    while (!Manager.DownloadCompleted)
        //    {
        //        DownloadedPiece downloadedPiece;
        //        var result = Manager.IncomingBlocks.TryDequeue(out downloadedPiece);
        //        if (result)
        //        {
        //            FileWriter.WriteDataTemp("a.pdf", downloadedPiece.Data, Manager.Torrent.PieceSize * downloadedPiece.Piece + (downloadedPiece.Block * TorrentManager.CHUNK_SIZE), Manager.Torrent.TotalSize);
        //            ConsoleUtil.Write("Downloaded %{0}", (Manager.Downloaded * 100 / Manager.Torrent.TotalSize));
        //        }

        //    }
        //}
    }
}