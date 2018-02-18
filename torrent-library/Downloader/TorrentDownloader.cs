using BencodeNET.Torrents;
using System;
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

        //public AnnounceResponse _AnnounceResponse { get; set; }
        //public AnnounceRequest _AnnounceRequest { get; set; }

        public byte[] PeerID { get; set; }
        public string InfoHash { get; set; }
        public Torrent _Torrent { get; set; }
        public TorrentInfo _TorrentInfo { get; set; }
        public List<Peer> ConnectedPeers { get; set; }
        public List<Peer> AllPeers { get; set; }
        public PeerHandshake PeerHandshakeRequest { get; set; }


        private List<Task> connectionTasks = new List<Task>();


        public TorrentDownloader(AnnounceRequest announceRequest, AnnounceResponse announceResponse, Torrent torrent, TorrentInfo torrentInfo)
        {
            PeerID = announceRequest.PeerID;
            InfoHash = announceRequest.InfoHash;
            _Torrent = torrent;
            _TorrentInfo = torrentInfo;
            ConnectedPeers = new List<Peer>();
            AllPeers = announceResponse.IPPort.Select(x => new Peer(x)).ToList();
            PeerHandshakeRequest = new PeerHandshake(PeerID, InfoHash);
        }

        public TorrentDownloader() { }

        public void StartDownload()
        {
            ConsoleUtil.Write("Connecting to peers...");
            _TorrentInfo.TotalSeeders += AllPeers.Count;

            // Connecting Peers
            ConnectToPeers();
        }

        private void ConnectToPeers()
        {
            List<Task> connectionTasks = new List<Task>();
            foreach (var peer in AllPeers)
            {
                var task = Task.Run(new Action(delegate ()
                {
                    var connected = peer.Connect();
                    if (connected)
                    {
                        ConnectedPeers.Add(peer);
                        peer.HandShake(PeerHandshakeRequest);
                    }
                        
                }));
                connectionTasks.Add(task);
            }

            Task.WhenAll(connectionTasks).ContinueWith(x =>
            {
                ConsoleUtil.Write("Connected to {0} peers.", ConnectedPeers.Count);
                _TorrentInfo.ConnectedSeeders = ConnectedPeers.Count;
            });
        }

    }
}
