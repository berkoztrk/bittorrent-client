using BencodeNET.Torrents;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using torrent_library.Model;
using torrent_library.Util;

namespace torrent_library.Downloader
{
    public class TorrentDownloader
    {
        private TorrentFileWriter FileWriter { get; set; }
        public TorrentManager Manager { get; set; }
        public double DLSpeed { get; set; }
        private System.Timers.Timer DLSpeedTimer { get; set; }
        private long[] DownloadSpeeds = new long[4];
        private int DLSpeedIndex { get; set; }
        public bool Paused { get; private set; }
        Task DownloaderTask;
        Task ConnectionTask;

        public TorrentDownloader(TorrentManager torrentInfo)
        {
            Manager = torrentInfo;
            this.FileWriter = new TorrentFileWriter();
            this.DLSpeed = 0;

            DLSpeedTimer = new System.Timers.Timer();
            DLSpeedTimer.Elapsed += DLSpeedTimer_Elapsed;
            DLSpeedTimer.Interval = 1000;
            DLSpeedTimer.Enabled = true;
            Paused = false;
        }

        public TorrentDownloader() { }

        private void DLSpeedTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            DownloadSpeeds[DLSpeedIndex] = Manager.DownloadedForSpeed;
            DLSpeedIndex = DLSpeedIndex == DownloadSpeeds.Length - 1 ? 0 : DLSpeedIndex + 1;
            DLSpeed = DownloadSpeeds.Average();
            ConsoleUtil.WriteSuccess("{0} Kb/s", Manager.DownloadedForSpeed);
            Manager.DownloadedForSpeed = 0;
        }

        public void StartDownload()
        {
            Download();
        }

        private void Download()
        {
            ConnectionTask = Task.Factory.StartNew(() => ConnectToPeers());
            DownloaderTask = Task.Factory.StartNew(() => DownloadFromConnectedPeers());
            Task.Factory.StartNew(() => WriteDownloadedDataToFile());
        }

        private void WriteDownloadedDataToFile()
        {
            while (Manager.Running)
            {
                RequestedBlock block;
                var result = Manager.FileQueue.TryDequeue(out block);
                if (result)
                {

                    var isPieceValid = SHA1Util.ValidatePiece(block, Manager.Torrent);

                    if (isPieceValid)
                    {
                        var files = block.GetFilePieceBelongs(Manager);
                        int offset = 0;
                        foreach (var file in files)
                        {
                            try
                            {
                                FileWriter.WriteData(file.FileInfo.FileName, block.Data.SubArray(offset, (int)file.Length), file.FileInfo.FileSize, file.FileOffset);
                                offset += (int)file.Length;
                                Manager.SaveAsJSON();
                            }
                            catch (Exception e)
                            {
                            }
                        }
                    }
                    else
                    {
                        Manager.ResetPieceProgess(block.Piece);
                    }

                }
            }
        }

        private void DownloadFromConnectedPeers()
        {
            while (!Manager.DownloadCompleted && !Paused)
            {

                var connectedPeers = Manager.Peers.Where(X => !X.Value.IsDisconnected).Select(x => x.Value).OrderByDescending(x => x.Rank);
                var rarestPieces = RequestedBlock.GetOrderedPiecesByRarest(Manager);
                foreach (var peer in connectedPeers)
                {
                    peer.SendKeepAlive();
                    peer.SendInterested();

                    while (peer.RequestCount < 5)
                    {
                        var requestBlock = RequestedBlock.GetBlock(peer, Manager, DownloadStrategy.RarestFirst, rarestPieces);
                        if (requestBlock != null && !peer.IsDisconnected)
                        {
                            peer.SendRequest(requestBlock);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            var _connectedPeers = Manager.Peers.Where(X => !X.Value.IsDisconnected).Select(x => x.Value).OrderByDescending(x => x.Rank);
            foreach (var peer in _connectedPeers)
            {
                peer.SendNotInterested();
            }

        }

        public void PauseDownload()
        {
            Paused = true;
        }

        public void ContinueDownload()
        {
            Paused = false;
            Download();
        }

        private void ConnectToPeers()
        {
            Manager._TorrentStatus = TorrentStatus.ConnectingToPeers;
            while (!Manager.DownloadCompleted && !Paused)
            {
                foreach (var kv in Manager.Peers)
                {
                    var peer = kv.Value;

                    if (peer.IsDisconnected)
                        peer.Connect();
                }

            }

        }
    }
}
