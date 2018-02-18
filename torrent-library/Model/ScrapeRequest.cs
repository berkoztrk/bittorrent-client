using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class ScrapeRequest
    {
        private const int ACTION = 2;
        public string InfoHash { get; set; }
        public long ConnectionID { get; set; }

        public ScrapeRequest(string infoHash, long ConnectionID)
        {
            this.InfoHash = infoHash;
            this.ConnectionID = ConnectionID;
        }

        public byte[] CreateScrapeRequestArray()
        {
            var _transactionID = new Random().Next();
            byte[] transactionID = BitConverterUtil.GetBytes(_transactionID);

            byte[] _action = BitConverterUtil.GetBytes(ACTION);
            byte[] _connectionID = BitConverterUtil.GetBytes(ConnectionID);
            byte[] _infoHash = BitConverterUtil.FromHexString(InfoHash);

            var requestArrayLength = transactionID.Length + _action.Length + _connectionID.Length + _infoHash.Length;
            var requestByteArray = new byte[requestArrayLength];

            Buffer.BlockCopy(_connectionID, 0, requestByteArray, 0, _connectionID.Length);
            Buffer.BlockCopy(_action, 0, requestByteArray, 8, _action.Length);
            Buffer.BlockCopy(transactionID, 0, requestByteArray, 12, transactionID.Length);
            Buffer.BlockCopy(_infoHash, 0, requestByteArray, 16, _infoHash.Length);


            return requestByteArray;

        }
    }
}
