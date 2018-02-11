using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
            public static readonly int Announce = 1;
            public static readonly int Scrape = 2;
            public static readonly int Error = 3;
        }

        public static class AnnounceAction
        {
            public static readonly int None = 0;
            public static readonly int Completed = 1;
            public static readonly int Started = 2;
            public static readonly int Stopped = 3;
        }


        private TrackerAdress _TrackerAddress { get; set; }


        public bool IsConnected
        {
            get; set;
        }
        public int NTimeout
        {
            get { return _nTimeout; }
            set
            {
                _nTimeout = value;
                CalculateReceiveTimeout();
            }
        }


        private int _nTimeout = 0;
        private int _ReceiveTimeout = 0;
        private long _ResponseConnectionID = -1;
        private string InfoHash { get; set; }
        private byte[] PeerID { get; set; }
        public AnnounceResponse _AnnounceResponse = null;
        public DateTime LastAnnounced = DateTime.MaxValue;


        public UDPTracker(TrackerAdress address, string infoHash)
        {
            _TrackerAddress = address;
            CalculateReceiveTimeout();
            InfoHash = infoHash;
            PeerID = CryptoUtil.RandomPeerID();

            IsConnected = false;
        }

        private void CalculateReceiveTimeout()
        {
            _ReceiveTimeout = (int)(15000 * Math.Pow((double)2, (double)_nTimeout));
        }

        public void ConnectToTracker()
        {
            try
            {
                Connect();
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.TimedOut && _nTimeout <= 8)
                {
                    NTimeout++;
                    ConnectToTracker();
                }
                else
                    throw e;
            }
        }

        public void Connect()
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

                NTimeout = 0;

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

        private byte[] CreateScrapeRequestArray()
        {
            var _transactionID = new Random().Next();
            byte[] transactionID = BitConverterUtil.GetBytes(_transactionID);

            byte[] _action = BitConverterUtil.GetBytes(Action.Scrape);
            byte[] _connectionID = BitConverterUtil.GetBytes(_ResponseConnectionID);
            byte[] _infoHash = BitConverterUtil.ConvertHexStringToByteArray(InfoHash);

            var requestArrayLength = transactionID.Length + _action.Length + _connectionID.Length + _infoHash.Length;
            var requestByteArray = new byte[requestArrayLength];

            Buffer.BlockCopy(_connectionID, 0, requestByteArray, 0, _connectionID.Length);
            Buffer.BlockCopy(_action, 0, requestByteArray, 8, _action.Length);
            Buffer.BlockCopy(transactionID, 0, requestByteArray, 12, transactionID.Length);
            Buffer.BlockCopy(_infoHash, 0, requestByteArray, 16, _infoHash.Length);


            return requestByteArray;

        }

        private byte[] CreateAnnounceByteArray()
        {
            //            IPv4 announce request:

            //            Offset Size    Name Value
            //0       64 - bit integer connection_id
            //8       32 - bit integer action          1 // announce
            //12      32 - bit integer transaction_id
            //16      20 - byte string info_hash
            //36      20 - byte string peer_id
            //56      64 - bit integer downloaded
            //64      64 - bit integer left
            //72      64 - bit integer uploaded
            //80      32 - bit integer  event           0 // 0: none; 1: completed; 2: started; 3: stopped
            //84      32-bit integer IP address      0 // default
            //88      32-bit integer key
            //92      32-bit integer num_want        -1 // default
            //96      16-bit integer port
            //98


            var _transactionID = new Random().Next();
            byte[] transactionID = BitConverterUtil.GetBytes(_transactionID);

            byte[] _action = BitConverterUtil.GetBytes(Action.Announce);
            byte[] _connectionID = BitConverterUtil.GetBytes(_ResponseConnectionID);
            byte[] _infoHash = BitConverterUtil.ConvertHexStringToByteArray(InfoHash);
            byte[] _downloaded = BitConverterUtil.GetBytes((long)0);
            byte[] _left = BitConverterUtil.GetBytes((long)0);
            byte[] _uploaded = BitConverterUtil.GetBytes((long)0);
            byte[] _event = BitConverterUtil.GetBytes(AnnounceAction.Started);
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


        public void Scrape()
        {
            ConnectToTracker();
            Announce();

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
            var client = new UdpClient(_TrackerAddress.Host, _TrackerAddress.Port);


            var requestByteArray = CreateScrapeRequestArray();


            var resp = client.Send(requestByteArray, requestByteArray.Length);
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var result = client.Receive(ref remoteEndPoint);


            var responseAction = result.SubArray(0, 4);
            var _action = BitConverterUtil.ToInt(responseAction);
            if (_action == Action.Error)
            {
                var errorMessage = BitConverterUtil.ToString(result.SubArray(8, result.Length - 8));
                throw new Exception(String.Format("I got error like this help!! => {0}", errorMessage));
            }


            var responseSeeders = result.SubArray(8, 4);
            var responseCompleted = result.SubArray(12, 4);
            var responseLeechers = result.SubArray(16, 4);
            var seederCount = BitConverterUtil.ToInt(responseSeeders);
            var completed = BitConverterUtil.ToInt(responseCompleted);
            var leecherCount = BitConverterUtil.ToInt(responseLeechers);

            Console.WriteLine("Seeders = {0}, Leechers {1}, Completed = {2}, ResultLength = {3}", seederCount, leecherCount, completed, result.Length);
        }

        public void Announce()
        {
            var requestByteArray = CreateAnnounceByteArray();

            var client = new UdpClient(_TrackerAddress.Host, _TrackerAddress.Port);

            var resp = client.Send(requestByteArray, requestByteArray.Length);
            var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var result = client.Receive(ref remoteEndPoint);

            _AnnounceResponse = new AnnounceResponse(result);
            LastAnnounced = DateTime.Now;
            var x = 5;
        }
    }
}
