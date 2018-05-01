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

    public enum TorrentStatus
    {
        ConnectingToTracker,
        Downloading,
        ConnectingToPeers,
        Finished
    }


    public class TorrentManager
    {

        public ConcurrentDictionary<string, Peer> Peers = new ConcurrentDictionary<string, Peer>();
        public ConcurrentQueue<RequestedBlock> FileQueue = new ConcurrentQueue<RequestedBlock>();
        public ConcurrentDictionary<int, byte[]> DownloadedPieces = new ConcurrentDictionary<int, byte[]>();
        public long DownloadedForSpeed { get; set; }
        private DateTime LastProgressSaved = DateTime.Now;

        public List<long> DownloadSpeeds = new List<long>();

        public byte[] PeerID { get; set; }
        public Torrent Torrent { get; set; }

        public bool[] Bitfield { get; set; }
        public long Downloaded { get; set; }
        public bool DownloadCompleted
        {
            get
            {
                if (PieceProgress.Where(x => !x).Count() == 0)
                {
                    _TorrentStatus = TorrentStatus.Finished;
                    return true;
                }
                else
                    return false;
            }
        }
        public bool[][] DownloadProgress;
        public bool[] PieceProgress
        {
            get
            {
                return DownloadProgress.Select(x => x.All(y => y)).ToArray();
            }
        }

        public bool Paused { get; internal set; }
        public bool Running = true;
        public TorrentStatus _TorrentStatus = TorrentStatus.ConnectingToTracker;


        public string GetStatusText()
        {
            switch (_TorrentStatus)
            {
                case TorrentStatus.ConnectingToTracker:
                    return "Connecting to trackers";
                case TorrentStatus.ConnectingToPeers:
                    return "Connecting to peers";
                case TorrentStatus.Downloading:
                    return "Downloading";
                case TorrentStatus.Finished:
                    return "Finished";
                default:
                    return "";
            }
        }


        private TorrentManager(byte[] peerID, TorrentWithTrackerInfo torrent)
        {
            this.PeerID = peerID;
            this.Torrent = torrent._Torrent;
            this.Downloaded = 0;
            this.Bitfield = new bool[Torrent.NumberOfPieces];
            this.DownloadProgress = new bool[Torrent.NumberOfPieces][];
            this.DownloadedForSpeed = 0;
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


        public void AddPeers(List<Peer> peers)
        {
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

        public void ResetPieceProgess(int piece)
        {
            for (int i = 0; i < DownloadProgress[piece].Length; i++)
            {
                DownloadProgress[piece][i] = false;
            }
            DownloadedPieces[piece] = new byte[TorrentPieceUtil.GetPieceSize(piece, Torrent)];
        }

        private void Peer_BlockReceived(object sender, RequestedBlock e)
        {
            var peer = sender as Peer;
            Peer _peer;

            if (!DownloadProgress[e.Piece][e.Block])
            {
                //if ((DateTime.Now - LastProgressSaved).TotalSeconds > 5)
                //{
                //    LastProgressSaved = DateTime.Now;
                //    SaveAsJSON();
                //}

                DownloadedForSpeed += e.Data.Length;
                Downloaded += e.Data.Length;

                if (!DownloadedPieces.ContainsKey(e.Piece))
                    DownloadedPieces[e.Piece] = new byte[TorrentPieceUtil.GetPieceSize(e.Piece, Torrent)];

                var blockSize = TorrentPieceUtil.GetBlockSize(e.Piece, 0, Torrent);
                Buffer.BlockCopy(e.Data, 0, DownloadedPieces[e.Piece], e.Block * blockSize, e.Data.Length);

                DownloadProgress[e.Piece][e.Block] = true;

                if (PieceProgress[e.Piece])
                {
                    byte[] val;
                    var result = DownloadedPieces.TryRemove(e.Piece, out val);
                    if (result)
                    {
                        var block = new RequestedBlock(e.Piece, -1, val.Length);
                        block.Data = val;
                        FileQueue.Enqueue(block);
                    }

                }
                foreach (var p in Peers)
                {
                    if (p.Value.IsBlockRequested[e.Piece][e.Block])
                        p.Value.SendCancel(e.Piece, e.Block * TorrentPieceUtil.CHUNK_SIZE, TorrentPieceUtil.GetBlockSize(e.Piece, e.Block, Torrent));
                }
            }

        }

        private void Peer_Disconnected(object sender, EventArgs e)
        {

        }

        public static TorrentManager Create(string infoHash, TorrentWithTrackerInfo twtInfo)
        {
            try
            {
                return FromJSON(infoHash, twtInfo);

            }
            catch (Exception e)
            {
                return new TorrentManager(PeerIDUtil.GenerateRandom(), twtInfo); ;
            }
        }

        public void RemoveTorrent()
        {
            var path = Settings.Instance.DownloadPath + @"/JSON/" + Torrent.OriginalInfoHash.ToUpperInvariant() + ".json";
            if (File.Exists(path))
                File.Delete(path);


            var torrentFilePath = Settings.Instance.DownloadPath + @"/" + Torrent.OriginalInfoHash.ToUpperInvariant() + ".torrent";
            if (File.Exists(torrentFilePath))
                File.Delete(torrentFilePath);
        }

        public void SaveAsJSON()
        {
            if (!Directory.Exists(Settings.Instance.DownloadPath + @"/JSON/"))
            {
                DirectoryInfo di = Directory.CreateDirectory(AppContext.BaseDirectory + @"/JSON/");
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            var path = Settings.Instance.DownloadPath + @"/JSON/" + Torrent.OriginalInfoHash.ToUpperInvariant() + ".json";
            var json = new JavaScriptSerializer().Serialize(new DownloadInfo()
            {
                PeerID = this.PeerID,
                Downloaded = this.Downloaded,
                DownloadProgress = this.DownloadProgress
            });
            File.WriteAllText(path, json);

        }

        public static TorrentManager FromJSON(string infoHASH, TorrentWithTrackerInfo twtInfo)
        {

            if (!Directory.Exists(Settings.Instance.DownloadPath + @"/JSON/"))
            {
                DirectoryInfo di = Directory.CreateDirectory(Settings.Instance.DownloadPath + @"/JSON/");
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            var path = Settings.Instance.DownloadPath + @"/JSON/" + infoHASH.ToUpperInvariant() + ".json";
            var json = File.ReadAllText(path);

            var jObject = JObject.Parse(json);
            var jToken = jObject.GetValue("DownloadProgress");
            var jToken2 = jObject.GetValue("PeerID");
            var downloaded = jObject.GetValue("Downloaded");
            var downloadProgress = jToken.ToObject(typeof(bool[][]));
            var peerID = jToken2.ToObject(typeof(byte[]));
            var _downloaded = downloaded;

            var obj = new DownloadInfo();
            obj.DownloadProgress = downloadProgress as bool[][];
            obj.PeerID = peerID as byte[];
            obj.Downloaded = _downloaded.ToObject<long>();

            var manager = new TorrentManager(obj, twtInfo);

            return manager;
        }


    }
}
