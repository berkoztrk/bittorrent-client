using BencodeNET.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class PeerHandshake
    {
        public const string PSTR = "BitTorrent protocol";
        public byte Length = 19;
        public string BittorrentProtocol = "";
        public string InfoHash { get; set; }
        public byte[] PeerID { get; set; }
        public byte[] Reserved { get; set; }
        public string PeerIDAsString { get; set; }

        public PeerHandshake() { }

        public PeerHandshake(byte[] peerID, string infoHash)
        {
            this.PeerID = peerID;
            this.Length = 19;
            this.BittorrentProtocol = "BitTorrent protocol";
            this.InfoHash = infoHash;
        }

        public byte[] GetAsByteArray()
        {
            var bittorrentProtocolLen = new byte[] { 19 };
            var bittorrentProtocol = Encoding.UTF8.GetBytes(BittorrentProtocol);
            var byteInfoHash = BitConverterUtil.FromHexString(InfoHash);
            var empty = new byte[8];

            var len = bittorrentProtocolLen.Length + bittorrentProtocol.Length + empty.Length + byteInfoHash.Length + PeerID.Length;

            byte[] downloadHandshakeRequest = new byte[len];
            Buffer.BlockCopy(bittorrentProtocolLen, 0, downloadHandshakeRequest, 0, bittorrentProtocolLen.Length);
            Buffer.BlockCopy(bittorrentProtocol, 0, downloadHandshakeRequest, bittorrentProtocolLen.Length, bittorrentProtocol.Length);
            Buffer.BlockCopy(empty, 0, downloadHandshakeRequest, bittorrentProtocol.Length + bittorrentProtocolLen.Length, empty.Length);
            Buffer.BlockCopy(byteInfoHash, 0, downloadHandshakeRequest, bittorrentProtocol.Length + empty.Length + bittorrentProtocolLen.Length, byteInfoHash.Length);
            Buffer.BlockCopy(PeerID, 0, downloadHandshakeRequest, bittorrentProtocol.Length + empty.Length + byteInfoHash.Length + bittorrentProtocolLen.Length, PeerID.Length);
            return downloadHandshakeRequest.ToArray();
        }

        public static PeerHandshake ConvertFromResponse(byte[] response)
        {
            var handshake = new PeerHandshake();
            handshake.Length = response[0];
            handshake.BittorrentProtocol = BitConverterUtil.ToString(response.SubArray(1, 19));
            handshake.Reserved = response.SubArray(20, 8);
            handshake.InfoHash = BitConverterUtil.ByteArrayToHexString(response.SubArray(28, 20));
            handshake.PeerIDAsString = Encoding.UTF8.GetString(response.SubArray(48, 20));
            handshake.PeerID = response.SubArray(48, 20);
            return handshake;
        }

        public override string ToString()
        {
            return Length.ToString() + BittorrentProtocol + InfoHash + PeerIDAsString;
        }
    }
}
