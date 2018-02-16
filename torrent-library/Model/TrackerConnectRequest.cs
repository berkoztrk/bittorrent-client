using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class TrackerConnectRequest
    {
        public int TransactionID { get; set; }

        public TrackerConnectRequest() { }

        public byte[] GetRequestArray()
        {
            byte[] request = new byte[16];

            TransactionID = new Random().Next();
            var transactionID = BitConverterUtil.GetBytes(TransactionID);
            var action = BitConverterUtil.GetBytes(TrackerAction.Connect);
            var protocolID = BitConverterUtil.GetBytes(0x41727101980);

            Buffer.BlockCopy(protocolID, 0, request, 0, protocolID.Length);
            Buffer.BlockCopy(action, 0, request, 8, action.Length);
            Buffer.BlockCopy(transactionID, 0, request, 12, transactionID.Length);

            return request;
        }
    }
}
