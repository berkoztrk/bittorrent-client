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

        public TorrentDownloader(TorrentManager torrentInfo)
        {
            Manager = torrentInfo;
            this.FileWriter = new TorrentFileWriter();
        }

        public TorrentDownloader() { }

        public void StartDownload()
        {
            Download();
        }

        private void Download()
        {
            Task.Factory.StartNew(() => ConnectToPeers());
            Task.Factory.StartNew(() => DownloadFromConnectedPeers());
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
                    FileWriter.WriteData("asdas.pdf", block.Data, TorrentPieceUtil.GetFileOffset(block, Manager.Torrent));
                }
                Thread.Sleep(50);
            }
        }

        private void DownloadFromConnectedPeers()
        {
            while (!Manager.DownloadCompleted)
            {
                var connectedPeers = Manager.Peers.Where(X => !X.Value.IsDisconnected).Select(x => x.Value);
                foreach (var peer in connectedPeers)
                {
                    peer.SendKeepAlive();
                    peer.SendInterested();

                    if (peer.IsPieceRequested)
                        continue;

                    var requestBlock = RequestedBlock.GetBlock(peer, Manager);
                    if (requestBlock != null)
                    {
                        peer.SendRequest(requestBlock);
                    }
                }
            }
        }

        private void ConnectToPeers()
        {
            while (!Manager.DownloadCompleted)
            {
                Manager.Peers.Select(x => x.Value)
                            .Where(x => !x.ConnectionRequestSent && x.IsDisconnected).ToList()
                            .ForEach(x => x.Connect());
                Thread.Sleep(500);
            }
        }
    }
}
