using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class TrackerConnectResponse
    {
        public int Action { get; set; }
        public long TransactionID { get; set; }
        public long ConnectionID { get; set; }

        public TrackerConnectResponse(TrackerConnectRequest request, byte[] result)
        {
            var responseAction = result.SubArray(0, 4);
            var responseTransactionID = result.SubArray(4, 4);
            var responseConnectionID = result.SubArray(8, 8).ToArray();

            Action = BitConverterUtil.ToInt(responseAction);
            TransactionID = BitConverterUtil.ToInt(responseTransactionID);
            ConnectionID = BitConverterUtil.ToLong(responseConnectionID);

            if (TransactionID != request.TransactionID)
                throw new Exception("Request and response transactionID's does not match.");

            if (Action != TrackerAction.Connect)
                throw new Exception("Response action is not Action.Connect");
        }

    }
}
