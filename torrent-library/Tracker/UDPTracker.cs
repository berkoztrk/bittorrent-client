using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Tracker
{
    public class UDPTracker
    {
        private TrackerAdress _TrackerAddress { get; set; }
        private int _nTimeout = 0;
        private int _ReceiveTimeout = 0;
        private long _ResponseConnectionID = -1;

        public UDPTracker(TrackerAdress address)
        {
            _TrackerAddress = address;
            _ReceiveTimeout = (int)(15000 * Math.Pow((double)2, (double)_nTimeout));
        }

        public void Connect()
        {
            UdpClient client = new UdpClient(_TrackerAddress.Host, _TrackerAddress.Port);
            client.Client.ReceiveTimeout = _ReceiveTimeout;

            byte[] request = new byte[16];
            byte[] transactionID = BitConverter.GetBytes(new Random().Next());
            byte[] action = BitConverter.GetBytes(0); // connect
            byte[] protocolID = BitConverter.GetBytes(0x41727101980).Reverse().ToArray();

            Buffer.BlockCopy(protocolID, 0, request, 0, protocolID.Length);
            Buffer.BlockCopy(action, 0, request, 8, action.Length);
            Buffer.BlockCopy(transactionID, 0, request, 12, transactionID.Length);

            var resp = client.Send(request, request.Length);
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var result = client.Receive(ref remoteEndPoint);

            var responseAction = result.SubArray(0, 4);
            var responseTransactionID = result.SubArray(4, 4);
            var responseConnectionID = result.SubArray(8, 8);

            var respAction = BitConverter.ToInt32(responseAction, 0);
            var respTransactionID = BitConverter.ToInt32(responseTransactionID, 0);
            var respConnectionID = BitConverter.ToInt64(responseConnectionID,0);
            _ResponseConnectionID = respConnectionID;
        }
    }
}
