using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class AnnounceRequest
    {
        public string InfoHash { get; set; }
        public long ConnectionID { get; set; }
        public int AnnounceAction { get; set; }
        public byte[] PeerID { get; set; }
        public Torrent _Torrent { get; set; }

        public AnnounceRequest(long connectionID, string infoHash, int announceAction, byte[] peerID, Torrent torrent)
        {
            this.InfoHash = infoHash;
            this.ConnectionID = connectionID;
            this.AnnounceAction = announceAction;
            this.PeerID = peerID;
            this._Torrent = torrent;
        }

        public byte[] GetRequestArray(long downloaded = 0)
        {
            var _transactionID = new Random().Next();
            byte[] transactionID = BitConverterUtil.GetBytes(_transactionID);

            byte[] _action = BitConverterUtil.GetBytes(1);
            byte[] _connectionID = BitConverterUtil.GetBytes(ConnectionID);
            byte[] _infoHash = BitConverterUtil.FromHexString(InfoHash);
            byte[] _downloaded = BitConverterUtil.GetBytes((long)0);
            byte[] _left = BitConverterUtil.GetBytes(downloaded == 0 ? _Torrent.TotalSize : downloaded);
            byte[] _uploaded = BitConverterUtil.GetBytes((long)0);
            byte[] _event = BitConverterUtil.GetBytes(AnnounceAction);
            byte[] _ip = BitConverterUtil.GetBytes(0);
            byte[] _key = BitConverterUtil.GetBytes(new Random().Next());
            byte[] _numwant = BitConverterUtil.GetBytes(-1);
            byte[] _port = BitConverterUtil.GetBytes(34367);

            var requestArrayLength = _action.Length + _connectionID.Length + transactionID.Length + PeerID.Length + _infoHash.Length + _downloaded.Length +
                _left.Length + _uploaded.Length + _event.Length + _ip.Length + _key.Length + _numwant.Length + _port.Length;
            var requestByteArray = new byte[requestArrayLength];


            Buffer.BlockCopy(_connectionID, 0, requestByteArray, 0, _connectionID.Length);
            Buffer.BlockCopy(_action, 0, requestByteArray, 8, _action.Length);
            Buffer.BlockCopy(transactionID, 0, requestByteArray, 12, transactionID.Length);
            Buffer.BlockCopy(_infoHash, 0, requestByteArray, 16, _infoHash.Length);
            Buffer.BlockCopy(PeerID, 0, requestByteArray, 36, PeerID.Length);
            Buffer.BlockCopy(_downloaded, 0, requestByteArray, 56, _downloaded.Length);
            Buffer.BlockCopy(_left, 0, requestByteArray, 64, _left.Length);
            Buffer.BlockCopy(_uploaded, 0, requestByteArray, 72, _uploaded.Length);
            Buffer.BlockCopy(_event, 0, requestByteArray, 80, _event.Length);
            Buffer.BlockCopy(_ip, 0, requestByteArray, 84, _ip.Length);
            Buffer.BlockCopy(_key, 0, requestByteArray, 88, _key.Length);
            Buffer.BlockCopy(_numwant, 0, requestByteArray, 92, _numwant.Length);
            Buffer.BlockCopy(_port, 0, requestByteArray, 96, _port.Length);

            return requestByteArray;
        }

       


    }
}
