using BencodeNET.Torrents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using torrent_library;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class TorrentManager
    {

        private static TorrentManager Instance;

        public ConcurrentDictionary<string, Peer> Peers = new ConcurrentDictionary<string, Peer>();
        public ConcurrentDictionary<int, Peer> DownloadingPieces = new ConcurrentDictionary<int, Peer>();
        public ConcurrentQueue<RequestedBlock> FileQueue = new ConcurrentQueue<RequestedBlock>();


        public byte[] PeerID { get; set; }
        public Torrent Torrent { get; set; }

        public bool[] Bitfield { get; set; }
        public long Downloaded { get; set; }
        public bool DownloadCompleted { get { return PieceProgress.Where(x => !x).Count() == 0; } }
        public bool[][] DownloadProgress;
        public bool[] PieceProgress
        {
            get
            {
                return DownloadProgress.Select(x => x.All(y => y)).ToArray();
            }
        }
        public bool Running = true;


        private TorrentManager(byte[] peerID, TorrentWithTrackerInfo torrent)
        {
            this.PeerID = peerID;
            this.Torrent = torrent._Torrent;
            this.Downloaded = 0;
            this.Bitfield = new bool[Torrent.NumberOfPieces];
            this.DownloadProgress = new bool[Torrent.NumberOfPieces][];
            for (int i = 0; i < Torrent.NumberOfPieces; i++)
            {
                DownloadProgress[i] = new bool[TorrentPieceUtil.GetBlockCount(i, Torrent)];
            }


        }
        public TorrentManager(DownloadInfo di, TorrentWithTrackerInfo twtInfo)
        {
            this.PeerID = di.PeerID;
            this.Torrent = twtInfo._Torrent;
            this.Downloaded = di.Downloaded;
            this.DownloadProgress = di.DownloadProgress;
        }

        private TorrentManager()
        {
        }
        public static TorrentManager GetInstance()
        {
            if (Instance == null)
            {
                throw new Exception("Instance is null");
            }

            return Instance;
        }

        public void AddPeers(List<Peer> peers)
        {
            var updated = false;
            foreach (var peer in peers)
            {
                var result = Peers.TryAdd(peer.IP, peer);
                if (result)
                {
                    peer.Disconnected += Peer_Disconnected;
                    peer.BlockReceived += Peer_BlockReceived;
                }
            }
        }

        private void Peer_BlockReceived(object sender, RequestedBlock e)
        {
            var peer = sender as Peer;
            Peer _peer;

            if (!DownloadProgress[e.Piece][e.Block])
            {
                DownloadProgress[e.Piece][e.Block] = true;
                FileQueue.Enqueue(e);
                foreach (var p in Peers)
                {
                    if (p.Value.IsBlockRequested[e.Piece][e.Block])
                        p.Value.SendCancel(e.Piece, e.Block * TorrentPieceUtil.CHUNK_SIZE, TorrentPieceUtil.GetBlockSize(e.Piece, e.Block, Torrent));
                }
            }

        }


        private void Peer_Disconnected(object sender, EventArgs e)
        {
            var peer = sender as Peer;
            var downloadingPeer = DownloadingPieces.Where(x => x.Value.IP == peer.IP);

            if (downloadingPeer.Count() > 0)
            {
                Peer _peer;
                DownloadingPieces.TryRemove(downloadingPeer.FirstOrDefault().Key, out _peer);
            }

        }

        public static TorrentManager Create(string infoHash, TorrentWithTrackerInfo twtInfo)
        {
            try
            {
                Instance = FromJSON(infoHash, twtInfo);
                return Instance;
            }
            catch (Exception e)
            {
                Instance = new TorrentManager(PeerIDUtil.GenerateRandom(), twtInfo);
                return Instance;
            }
        }

        public void SaveAsJSON()
        {
            var path = @"C:/torrents/" + Torrent.OriginalInfoHash.ToUpperInvariant() + ".json";
            var json = new JavaScriptSerializer().Serialize(new DownloadInfo()
            {
                PeerID = this.PeerID,
                Downloaded = this.Downloaded,
                //DownloadProgress = this.DownloadProgress
            });

            File.WriteAllText(path, json);
        }

        public static TorrentManager FromJSON(string infoHASH, TorrentWithTrackerInfo twtInfo)
        {
            var path = @"C:/torrents/" + infoHASH.ToUpperInvariant() + ".json";
            var json = File.ReadAllText(path);
            //var obj = JsonConvert.DeserializeObject<DownloadInfo>(json.Replace("[[", "[").Replace("]]", "]"));

            var jObject = JObject.Parse(json);
            var jToken = jObject.GetValue("DownloadProgress");
            var jToken2 = jObject.GetValue("PeerID");
            var downloadProgress = jToken.ToObject(typeof(bool[][]));
            var peerID = jToken2.ToObject(typeof(byte[]));

            var obj = new DownloadInfo();
            obj.DownloadProgress = downloadProgress as bool[][];
            obj.PeerID = peerID as byte[];

            var manager = new TorrentManager(obj, twtInfo);
            //PropTwoClass value = jToken.ToObject(typeof(PropTwoClass));

            //var manager = new TorrentManager(obj, twtInfo);

            return manager;
            //return manager;
        }


    }
}
