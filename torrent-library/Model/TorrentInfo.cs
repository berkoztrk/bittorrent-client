using BencodeNET.Torrents;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Model
{
    public class TorrentManager
    {
        public byte[] PeerID { get; set; }
        public Torrent _Torrent { get; set; }
        public int TotalPeers { get; set; }
        public int CurrentOffset { get; set; }
        public int CurrentPiece { get; set; }
        public BlockingCollection<Peer> ConnectedPeers { get; set; }
        public BlockingCollection<Peer> NotConnectedPeers { get; set; }
        public List<string> FilePaths { get; set; }
        public int FileIndex { get; set; }

        public long TotalDownloaded
        {
            get
            {
                return (CurrentPiece * _Torrent.PieceSize) + CurrentOffset;
            }
        }

        public bool FilesCreatedSuccessfully { get; set; }

        public bool DownloadCompleted
        {
            get
            {
                return TotalDownloaded == _Torrent.TotalSize;
            }
        }

        public TorrentManager(Torrent torrent)
        {
            PeerID = PeerIDUtil.GenerateRandom();
            TotalPeers = 0;
            CurrentOffset = 0;
            CurrentPiece = 0;
            ConnectedPeers = new BlockingCollection<Peer>();
            NotConnectedPeers = new BlockingCollection<Peer>();
            _Torrent = torrent;
            FilePaths = new List<string>();
            FileIndex = 0;
            FilesCreatedSuccessfully = false;


        }
    }
}
