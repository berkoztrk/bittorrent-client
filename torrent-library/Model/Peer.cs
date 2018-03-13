using BencodeNET.Torrents;
using ReactiveSockets;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class Peer
    {

        public enum MessageType : int
        {
            Unknown = -3,
            Handshake = -2,
            KeepAlive = -1,
            Choke = 0,
            Unchoke = 1,
            Interested = 2,
            NotInterested = 3,
            Have = 4,
            Bitfield = 5,
            Request = 6,
            Piece = 7,
            Cancel = 8,
            Port = 9,
        }

        private const int BUFFER_SIZE = 256;

        public event EventHandler StateChanged;
        public event EventHandler Disconnected;
        public event EventHandler<RequestedBlock> BlockReceived;

        private TorrentManager Manager;

        public string ID { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public bool IsHandshakeReceived { get; private set; }

        private TcpClient _TcpClient { get; set; }
        private NetworkStream _NetworkStream { get; set; }
        public bool IsDisconnected { get; private set; }
        public bool IsInterestedSent { get; private set; }
        public bool IsChokeReceived { get; private set; }
        public DateTime LastKeepAlive { get; private set; }

        public bool[] Bitfield;
        private List<byte> data = new List<byte>();
        private byte[] streamBuffer = new byte[BUFFER_SIZE];
        public DateTime LastActive;

        public bool ConnectionRequestSent;

        public bool[][] IsBlockRequested { get; set; }
        public bool IsPieceRequested = false;
        private int requestCount = 0;
        public int RequestCount
        {
            get
            {
                return this.requestCount;
            }
        }
        public int Rank = 0;

        public override string ToString()
        {
            return IP + ":" + Port;
        }

        public Peer(IPPortPair pair)
        {
            IP = pair.IP.ToString();
            Port = pair.Port;
            IsDisconnected = true;
            Manager = TorrentManager.GetInstance();
            Bitfield = new bool[Manager.Torrent.NumberOfPieces];
            IsBlockRequested = new bool[Manager.Torrent.NumberOfPieces][];
            for (int i = 0; i < IsBlockRequested.Length; i++)
            {
                IsBlockRequested[i] = new bool[TorrentPieceUtil.GetBlockCount(i, Manager.Torrent)];
            }
        }

        public void Connect()
        {
            if (_TcpClient == null)
            {
                _TcpClient = new TcpClient();

                try
                {
                    ConnectionRequestSent = true;
                    _TcpClient.ConnectAsync(IP, Port).ContinueWith(x =>
                    {
                        try
                        {
                            IsDisconnected = false;
                            ConsoleUtil.Write("Connected => {0}:{1}", IP, Port);

                            _NetworkStream = _TcpClient.GetStream();
                            _NetworkStream.BeginRead(streamBuffer, 0, BUFFER_SIZE, new AsyncCallback(HandleRead), null);
                            SendHandshake();
                        }
                        catch (Exception e)
                        {
                            Disconnect();
                            return;
                        }
                    });
                }
                catch (Exception e)
                {
                    Disconnect();
                    return;
                }

            }
        }

        public void SendRequest(RequestedBlock block)
        {
            Interlocked.Increment(ref this.requestCount);
            ConsoleUtil.Write(IP + ":" + Port + "-> request  " + block.ToString() + " " + RequestCount + " " + this.requestCount);
            IsBlockRequested[block.Piece][block.Block] = true;
            IsPieceRequested = true;
            SendBytes(EncodeRequest(block.Piece, block.Block * TorrentPieceUtil.CHUNK_SIZE, TorrentPieceUtil.GetBlockSize(block.Piece, block.Block, Manager.Torrent)));

        }

        public static byte[] EncodeNotInterested()
        {
            return EncodeState(MessageType.NotInterested);
        }

        internal void SendNotInterested()
        {
            if (!IsInterestedSent)
                return;

            ConsoleUtil.Write(this.ToString() + "-> not interested");
            SendBytes(EncodeNotInterested());
            IsInterestedSent = false;
        }

        public void SendCancel(int index, long begin, int length)
        {
            IsPieceRequested = false;

            var block = (int)begin / TorrentPieceUtil.CHUNK_SIZE;
            if (this.requestCount > 0)
                Interlocked.Decrement(ref this.requestCount);
            //RequestCount = RequestCount > 0 ? RequestCount - 1 : RequestCount;

            SendBytes(EncodeCancel(index, begin, length));
        }

        private byte[] EncodeCancel(int index, long begin, int length)
        {
            byte[] message = new byte[17];
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(13), 0, message, 0, 4);
            message[4] = (byte)MessageType.Cancel;
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(index), 0, message, 5, 4);
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(begin), 0, message, 9, 4);
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(length), 0, message, 13, 4);

            return message;
        }

        private byte[] EncodeRequest(int index, int begin, int length)
        {
            byte[] message = new byte[17];
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(13), 0, message, 0, 4);
            message[4] = (byte)MessageType.Request;
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(index), 0, message, 5, 4);
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(begin), 0, message, 9, 4);
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(length), 0, message, 13, 4);

            return message;
        }

        private void HandleRead(IAsyncResult ar)
        {
            int bytes = 0;
            try
            {
                bytes = _NetworkStream.EndRead(ar);
            }
            catch (Exception e)
            {
                Disconnect();
                return;
            }

            data.AddRange(streamBuffer.Take(bytes));

            int messageLength = GetMessageLength(data);
            while (data.Count >= messageLength)
            {
                HandleMessage(data.Take(messageLength).ToArray());
                data = data.Skip(messageLength).ToList();

                messageLength = GetMessageLength(data);
            }

            try
            {
                _NetworkStream.BeginRead(streamBuffer, 0, BUFFER_SIZE, new AsyncCallback(HandleRead), null);
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }

        public void SendInterested()
        {
            if (IsInterestedSent)
                return;

            SendBytes(EncodeInterested());
            IsInterestedSent = true;
        }

        public static byte[] EncodeInterested()
        {
            return EncodeState(MessageType.Interested);
        }

        public static byte[] EncodeState(MessageType type)
        {
            byte[] message = new byte[5];
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(1), 0, message, 0, 4);
            message[4] = (byte)type;
            return message;
        }

        private MessageType GetMessageType(byte[] bytes)
        {
            if (!IsHandshakeReceived)
                return MessageType.Handshake;

            if (bytes.Length == 4 && EndianBitConverter.Big.ToInt32(bytes, 0) == 0)
                return MessageType.KeepAlive;

            if (bytes.Length > 4 && Enum.IsDefined(typeof(MessageType), (int)bytes[4]))
                return (MessageType)bytes[4];

            return MessageType.Unknown;
        }

        private void HandleBitfield(bool[] isPieceDownloaded)
        {
            for (int i = 0; i < Manager.Torrent.NumberOfPieces; i++)
                Bitfield[i] = Bitfield[i] || isPieceDownloaded[i];

        }

        public static bool DecodeBitfield(byte[] bytes, int pieces, out bool[] isPieceDownloaded)
        {
            isPieceDownloaded = new bool[pieces];

            int expectedLength = Convert.ToInt32(Math.Ceiling(pieces / 8.0)) + 1;

            if (bytes.Length != expectedLength + 4 || EndianBitConverter.Big.ToInt32(bytes, 0) != expectedLength)
            {
                return false;
            }

            BitArray bitfield = new BitArray(bytes.Skip(5).ToArray());

            for (int i = 0; i < pieces; i++)
                isPieceDownloaded[i] = bitfield[bitfield.Length - 1 - i];

            return true;
        }

        private void HandleMessage(byte[] bytes)
        {
            LastActive = DateTime.UtcNow;

            MessageType type = GetMessageType(bytes);

            if (type == MessageType.Unknown)
            {
                return;
            }
            else if (type == MessageType.Handshake)
            {
                var response = PeerHandshake.ConvertFromResponse(bytes);
                HandleHandshake(response);
                return;
            }
            else if (type == MessageType.Bitfield)
            {
                bool[] isPieceDownloaded;
                if (DecodeBitfield(bytes, Manager.Torrent.NumberOfPieces, out isPieceDownloaded))
                {
                    HandleBitfield(isPieceDownloaded);
                }
                return;
            }
            else if (type == MessageType.KeepAlive && DecodeKeepAlive(bytes))
            {
                HandleKeepAlive();
                return;
            }
            else if (type == MessageType.Choke && DecodeChoke(bytes))
            {
                HandleChoke();
                return;
            }
            else if (type == MessageType.Unchoke && DecodeUnchoke(bytes))
            {
                HandleUnchoke();
                return;
            }
            else if (type == MessageType.Have)
            {
                int index;
                if (DecodeHave(bytes, out index))
                {
                    HandleHave(index);
                    return;
                }
            }
            else if (type == MessageType.Piece)
            {
                int index;
                int begin;
                byte[] data;
                if (DecodePiece(bytes, out index, out begin, out data))
                {
                    HandlePiece(index, begin, data);
                    return;
                }
            }

            //Disconnect();
        }

        private void HandlePiece(int index, int begin, byte[] data)
        {
            var block = begin / TorrentPieceUtil.CHUNK_SIZE;

            ConsoleUtil.Write(IP + ":" + Port + " <- piece " + index + ", " + block + ", " + data.Length);
            IsPieceRequested = false;
            if (this.requestCount > 0)
                Interlocked.Decrement(ref this.requestCount);
            //RequestCount = RequestCount > 0 ? RequestCount - 1 : RequestCount;

            var reqBlock = new RequestedBlock(index, block, TorrentPieceUtil.GetBlockSize(0, 0, Manager.Torrent));
            reqBlock.Data = data;

            Rank++;

            if (BlockReceived != null)
                BlockReceived(this, reqBlock);

        }

        private bool DecodePiece(byte[] bytes, out int index, out int begin, out byte[] data)
        {
            index = -1;
            begin = -1;
            data = new byte[0];

            if (bytes.Length < 13)
            {
                ConsoleUtil.Write("invalid piece message");
                return false;
            }

            int length = EndianBitConverter.Big.ToInt32(bytes, 0) - 9;
            index = EndianBitConverter.Big.ToInt32(bytes, 5);
            begin = EndianBitConverter.Big.ToInt32(bytes, 9);

            data = new byte[length];
            Buffer.BlockCopy(bytes, 13, data, 0, length);

            return true;
        }

        private void HandleHave(int index)
        {
            Bitfield[index] = true;
            ConsoleUtil.Write(IP + ":" + Port + "<- have " + index);

            var handler = StateChanged;
            if (handler != null)
                handler(this, new EventArgs());
        }

        private bool DecodeHave(byte[] bytes, out int index)
        {
            index = -1;

            if (bytes.Length != 9 || EndianBitConverter.Big.ToInt32(bytes, 0) != 5)
            {
                ConsoleUtil.Write("invalid have, first byte must equal 0x2");
                return false;
            }

            index = EndianBitConverter.Big.ToInt32(bytes, 5);

            return true;
        }

        private void HandleUnchoke()
        {
            ConsoleUtil.Write("{0}:{1} unchoke", IP, Port);
            IsChokeReceived = false;

            var handler = StateChanged;
            if (handler != null)
                handler(this, new EventArgs());
        }

        private bool DecodeUnchoke(byte[] bytes)
        {
            return DecodeState(bytes, MessageType.Unchoke);
        }

        private void HandleChoke()
        {
            IsChokeReceived = true;

            var handler = StateChanged;
            if (handler != null)
                handler(this, new EventArgs());
        }

        private bool DecodeChoke(byte[] bytes)
        {
            return DecodeState(bytes, MessageType.Choke);
        }

        private bool DecodeState(byte[] bytes, MessageType type)
        {
            if (bytes.Length != 5 || EndianBitConverter.Big.ToInt32(bytes, 0) != 1 || bytes[4] != (byte)type)
            {
                ConsoleUtil.Write("invalid " + Enum.GetName(typeof(MessageType), type));
                return false;
            }
            return true;
        }

        private void HandleKeepAlive()
        {
            ConsoleUtil.Write("Got keep alive {0}:{1}", IP, Port);
        }

        public void SendKeepAlive()
        {
            if (LastKeepAlive > DateTime.UtcNow.AddSeconds(-30))
                return;

            ConsoleUtil.Write(IP + ":" + Port + "-> keep alive");
            SendBytes(EncodeKeepAlive());
            LastKeepAlive = DateTime.UtcNow;
        }

        private byte[] EncodeKeepAlive()
        {
            return EndianBitConverter.Big.GetBytes(0);
        }

        private bool DecodeKeepAlive(byte[] bytes)
        {
            if (bytes.Length != 4 || EndianBitConverter.Big.ToInt32(bytes, 0) != 0)
            {
                ConsoleUtil.Write("invalid keep alive");
                return false;
            }
            return true;
        }

        private int GetMessageLength(List<byte> data)
        {
            if (!IsHandshakeReceived)
                return 68;

            if (data.Count < 4)
                return int.MaxValue;

            return EndianBitConverter.Big.ToInt32(data.ToArray(), 0) + 4;
        }


        public void Disconnect()
        {
            if (!IsDisconnected)
            {
                IsDisconnected = true;
                ConsoleUtil.Write("Disconnected => {0}:{1}", IP, Port);
            }

            if (_TcpClient != null)
                _TcpClient.Close();

            ConnectionRequestSent = false;
            IsInterestedSent = false;
            IsHandshakeReceived = false;
            IsPieceRequested = false;
            //requestCount = 0;
            Interlocked.Exchange(ref this.requestCount, 0);
            for (int i = 0; i < IsBlockRequested.Length; i++)
            {
                IsBlockRequested[i] = new bool[TorrentPieceUtil.GetBlockCount(i, Manager.Torrent)];
            }

            if (Disconnected != null)
                Disconnected(this, new EventArgs());
        }

        private void SendHandshake()
        {
            var hsRequest = new PeerHandshake(Manager.PeerID, Manager.Torrent.OriginalInfoHash).GetAsByteArray();
            SendBytes(hsRequest);

        }

        private void SendBytes(byte[] bytes)
        {
            try
            {
                _NetworkStream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception e)
            {
                Disconnect();
            }
        }

        private void HandleHandshake(PeerHandshake hs)
        {
            if (hs == null || Manager.Torrent.OriginalInfoHash.ToUpperInvariant() != hs.InfoHash.ToUpperInvariant())
            {
                Disconnect();
                return;
            }

            ID = hs.PeerIDAsString;

            IsHandshakeReceived = true;
            SendBitfield();
        }

        private void SendBitfield()
        {
            var buffer = new byte[256];
            //SendBytes(EncodeBitfield(Manager.Bitfield));
        }

        private static IEnumerable<bool> GetBits(byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return (b & 0x80) != 0;
                b *= 2;
            }
        }

        private static byte[] EncodeBitfield(bool[] isPieceDownloaded)
        {
            int numPieces = isPieceDownloaded.Length;
            int numBytes = Convert.ToInt32(Math.Ceiling(numPieces / 8.0));
            int numBits = numBytes * 8;

            int length = numBytes + 1;

            byte[] message = new byte[length + 4];
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(length), 0, message, 0, 4);
            message[4] = (byte)MessageType.Bitfield;

            bool[] downloaded = new bool[numBits];
            for (int i = 0; i < numPieces; i++)
                downloaded[i] = isPieceDownloaded[i];

            BitArray bitfield = new BitArray(downloaded);
            BitArray reversed = new BitArray(numBits);
            for (int i = 0; i < numBits; i++)
                reversed[i] = bitfield[numBits - i - 1];

            reversed.CopyTo(message, 5);

            return message;
        }
    }
}