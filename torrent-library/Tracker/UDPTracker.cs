using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Model;
using torrent_library.Util;

namespace torrent_library.Tracker
{
    public class UDPTracker
    {
        public static class Action
        {
            public static readonly int Connect = 0;
            public static readonly int Scrape = 2;
        }

        private TrackerAdress _TrackerAddress { get; set; }
        private int _nTimeout = 0;
        private int _ReceiveTimeout = 0;
        private long _ResponseConnectionID = -1;

        public bool IsConnected { get; set; }

        public UDPTracker(TrackerAdress address)
        {
            _TrackerAddress = address;
            _ReceiveTimeout = (int)(15000 * Math.Pow((double)2, (double)_nTimeout));

            IsConnected = false;
        }

        public void Connect()
        {
            try
            {
                using (UdpClient client = new UdpClient(_TrackerAddress.Host, _TrackerAddress.Port))
                {
                    client.Client.ReceiveTimeout = _ReceiveTimeout;

                    byte[] request = new byte[16];

                    var _transactionID = new Random().Next();
                    byte[] transactionID;
                    byte[] action;
                    byte[] protocolID;

                    transactionID = BitConverterUtil.GetBytes(_transactionID);
                    action = BitConverterUtil.GetBytes(Action.Connect);
                    protocolID = BitConverterUtil.GetBytes(0x41727101980);

                    Buffer.BlockCopy(protocolID, 0, request, 0, protocolID.Length);
                    Buffer.BlockCopy(action, 0, request, 8, action.Length);
                    Buffer.BlockCopy(transactionID, 0, request, 12, transactionID.Length);

                    var resp = client.Send(request, request.Length);
                    var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                    var result = client.Receive(ref remoteEndPoint);

                    if (result.Length < 16)
                        throw new Exception("Couldn't received response correctly.");

                    var responseAction = result.SubArray(0, 4);
                    var responseTransactionID = result.SubArray(4, 4);
                    var responseConnectionID = result.SubArray(8, 8).ToArray();

                    var respAction = BitConverterUtil.ToInt(responseAction);
                    var respTransactionID = BitConverterUtil.ToInt(responseTransactionID);
                    var respConnectionID = BitConverterUtil.ToLong(responseConnectionID);

                    if (respTransactionID != _transactionID)
                        throw new Exception("Request and response transactionID's does not match.");

                    if (respAction != Action.Connect)
                        throw new Exception("Response action is not Action.Connect");

                    _ResponseConnectionID = respConnectionID;
                    IsConnected = true;
                }
            }
            catch (Exception e)
            {

            }
        }

        public ScrapeResponse Scrape(string infoHash)
        {
            using (UdpClient client = new UdpClient(_TrackerAddress.Host, _TrackerAddress.Port))
            {
                //0               64 - bit integer connection_id
                //8               32 - bit integer action          2 // scrape
                //12              32 - bit integer transaction_id
                //16 + 20 * n     20 - byte string info_hash
                //16 + 20 * N


                //                scrape response:
                //Offset Size            Name Value
                //0           32 - bit integer action          2 // scrape
                //4           32 - bit integer transaction_id
                //8 + 12 * n  32 - bit integer seeders
                //12 + 12 * n 32 - bit integer completed
                //16 + 12 * n 32 - bit integer leechers
                //8 + 12 * N

                //client.Client.ReceiveTimeout = _ReceiveTimeout;

                var _transactionID = new Random().Next();
                byte[] transactionID = BitConverterUtil.GetBytes(_transactionID);
                byte[] action = BitConverterUtil.GetBytes(Action.Scrape); // connect
                byte[] connectionID = BitConverterUtil.GetBytes(_ResponseConnectionID).ToArray();
                byte[] _infoHash = BitConverterUtil.GetBytes(infoHash);

                int requestArrayLength = transactionID.Length + action.Length + connectionID.Length + _infoHash.Length;
                byte[] requestByteArray = new byte[requestArrayLength];

                Buffer.BlockCopy(connectionID, 0, requestByteArray, 0, connectionID.Length);
                Buffer.BlockCopy(action, 0, requestByteArray, 8, action.Length);
                Buffer.BlockCopy(transactionID, 0, requestByteArray, 12, transactionID.Length);
                Buffer.BlockCopy(_infoHash, 0, requestByteArray, 16, _infoHash.Length);

                var resp = client.Send(requestByteArray, requestByteArray.Length);
                var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var result = client.Receive(ref remoteEndPoint);

                var N = (result.Length - 8) / 12;

                var responseSeeders = result.SubArray(8, 4);
                var responseCompleted = result.SubArray(12, 4);
                var responseLeechers = result.SubArray(16, result.Length - 16);

                var seederCount = BitConverterUtil.ToInt(responseSeeders);
                var completed = BitConverterUtil.ToInt(responseCompleted);
                var leecherCount = BitConverterUtil.ToInt(responseLeechers);


                return new ScrapeResponse()
                {
                    Seeders = seederCount,
                    Leechers = leecherCount,
                    Completed = completed
                };
            }
        }
    }
}
