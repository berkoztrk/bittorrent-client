using BencodeNET.Torrents;
using System;
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

        public byte[] PeerID { get; set; }
        public string InfoHash { get; set; }
        public Torrent _Torrent { get; set; }
        public TorrentManager _TorrentInfo { get; set; }
        public PeerHandshake PeerHandshakeRequest { get; set; }

        private List<Task> connectionTasks = new List<Task>();


        public TorrentDownloader(Torrent torrent, TorrentManager torrentInfo)
        {
            PeerID = torrentInfo.PeerID;
            InfoHash = torrent.OriginalInfoHash;
            _Torrent = torrent;
            _TorrentInfo = torrentInfo;
            PeerHandshakeRequest = new PeerHandshake(PeerID, InfoHash);
        }

        public TorrentDownloader() { }

        private void DownloadPiece()
        {
            while (!_TorrentInfo.DownloadCompleted)
            {

                foreach (var connectedPeer in _TorrentInfo.NotConnectedPeers.Where(x => x.Handshaked))
                {
                    do
                    {
                        if (connectedPeer.Unchoked )
                        {
                            connectedPeer.SendRequest();
                            ConsoleUtil.Write("Sent download request to peer " + connectedPeer.UniqueID.ToString());
                        }
                    } while (connectedPeer.Unchoked);
                    
                }
            }

        }

        public void StartDownload()
        {
            // Connecting Peers
            Task.Run(() => ConnectToPeers(_TorrentInfo.NotConnectedPeers));

            Task.Run(() => DownloadPiece());
        }

        private void ConnectToPeers(BlockingCollection<Peer> notConnectedPeers)
        {
            if (notConnectedPeers.Count > 0)
                ConsoleUtil.Write("Connecting to peers...");

            List<Task> connectionTasks = new List<Task>();
            foreach (var peer in notConnectedPeers.Where(x => !x.Unchoked))
            {
                var task = Task.Run(() =>
                {
                    var connected = peer.Connect();
                    if (connected)
                    {
                        peer.HandShake(PeerHandshakeRequest);
                        if (peer.Handshaked)
                        {
                            var response = peer.SendInterested();
                        }
                    }
                });
                connectionTasks.Add(task);
            }

            Task.WhenAll(connectionTasks).ContinueWith(x =>
                {
                    if (!_TorrentInfo.DownloadCompleted)
                        ConnectToPeers(_TorrentInfo.NotConnectedPeers);
                });
        }

    }
}
