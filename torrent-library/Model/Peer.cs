using BencodeNET.Torrents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class Peer
    {
        public Guid UniqueID { get; set; }
        private const int CON_TIMEOUT = 2; // seconds
        public IPPortPair _IPPortPair { get; set; }
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public bool Handshaked { get; set; }
        public int PerformancePoint { get; set; }
        public TcpClient Client { get; set; }
        private Torrent _Torrent { get; set; }
        private TorrentManager _TorrentInfo { get; set; }
        public BitArray Bitfield { get; set; }
        private int PieceLen { get; set; }
        private TorrentFileWriter FileWriter { get; set; }
        public bool Unchoked { get; set; }
        public bool? PieceResponseReceived { get; set; }

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

        public Peer(IPPortPair pair, Torrent torrent, TorrentManager torrentInfo)
        {
            _IPPortPair = pair;
            IP = pair.IP;
            Port = pair.Port;
            Handshaked = false;
            _Torrent = torrent;
            _TorrentInfo = torrentInfo;
            UniqueID = Guid.NewGuid();
            FileWriter = new TorrentFileWriter();
            PerformancePoint = 1;
            Unchoked = false;
            PieceResponseReceived = null;
        }


        public static bool operator ==(Peer obj1, Peer obj2)
        {
            return obj1.UniqueID == obj2.UniqueID;
        }

        public static bool operator !=(Peer obj1, Peer obj2)
        {
            return obj1.UniqueID != obj2.UniqueID;
        }

        public bool SendInterested()
        {
            try
            {
                if (!Handshaked)
                    throw new Exception("Peer not handshaked.");

                byte[] interestedMsg = new byte[5] { 0, 0, 0, 1, 2 };
                byte[] buffer = new byte[1024];

                var stream = Client.GetStream();
                stream.Write(interestedMsg, 0, interestedMsg.Length);
                stream.Read(buffer, 0, buffer.Length);

                if (IsUnchoking(buffer))
                {
                    //_TorrentInfo.NotConnectedPeers.GetConsumingEnumerable().ToList().Remove(this);
                    //_TorrentInfo.ConnectedPeers.Add(this);
                    Unchoked = true;
                    return true;
                }
                else
                {
                    //_TorrentInfo.ConnectedPeers.GetConsumingEnumerable().ToList().Remove(this);
                    //_TorrentInfo.NotConnectedPeers.Add(this);
                    Disconnect();
                    Unchoked = false;
                    return false;
                }


            }
            catch (Exception e)
            {
                return false;
            }
        }

        private void Disconnect()
        {
            Handshaked = false;
            Unchoked = false;
            Client = new TcpClient();
        }

        private bool IsUnchoking(byte[] response)
        {
            return response[0] == 0 && response[1] == 0 && response[2] == 0 && response[3] == 1 && response[4] == 1;
        }

        private bool IsChoking(byte[] response)
        {
            return response[0] == 0 && response[1] == 0 && response[2] == 0 && response[3] == 1 && response[4] == 0;
        }

        private bool IsPieceResponse(byte[] response)
        {
            return response[4] == (int)MessageType.Piece;
        }



        public bool SendRequest()
        {
            try
            {
                PieceResponseReceived = false;
                var currentPiece = _TorrentInfo.CurrentPiece;

                if (Bitfield[currentPiece])
                {
                    int bufferLength = 16384;
                    var request = EncodeRequest(currentPiece, _TorrentInfo.CurrentOffset, bufferLength);
                    byte[] buffer = new byte[bufferLength];
                    var stream = Client.GetStream();
                    stream.Write(request, 0, request.Length);
                    stream.Read(buffer, 0, buffer.Length);

                    if (IsPieceResponse(buffer))
                    {
                        buffer = buffer.SubArray(5, bufferLength - 5);
                        PieceResponseReceived = true;
                    }
                    else
                    {
                        Disconnect(); return false;
                    }
                        

                    bufferLength = buffer.Length;
                    var newOffset = _TorrentInfo.CurrentOffset + buffer.Length;
                    int actualDataLength = bufferLength;


                    if (newOffset >= _Torrent.PieceSize)
                    {
                        actualDataLength = ((int)_Torrent.PieceSize - _TorrentInfo.CurrentOffset);
                        _TorrentInfo.CurrentOffset = 0;
                        _TorrentInfo.CurrentPiece++;
                        if (_TorrentInfo.CurrentPiece == _Torrent.NumberOfPieces)
                        {
                            _TorrentInfo.DownloadCompleted = true;
                            return false;
                        }
                    }
                    else
                    {
                        _TorrentInfo.CurrentOffset = newOffset;
                    }

                    buffer = buffer.SubArray(0, actualDataLength);

                    try
                    {
                        FileWriter.WriteData(_Torrent.Files[0].FileName, buffer);
                        ConsoleUtil.Write("Downloaded {0} bytes of data", buffer.Length);
                    }
                    catch (Exception e)
                    {
                        var x = 5;
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        private bool ConnectToPeer()
        {
            Client = new TcpClient();
            try
            {
                var result = Client.BeginConnect(IP, Port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(CON_TIMEOUT));
                if (success)
                {
                    return true;
                }
                else
                    throw new Exception("Peer connection timed out.");
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool Connect()
        {

            try
            {
                return ConnectToPeer();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void HandShake(PeerHandshake handshakeRequest)
        {
            try
            {
                bool connected = ConnectToPeer();
                if (connected)
                {
                    byte[] buffer = new byte[68];
                    var handshakeRequestByteArray = handshakeRequest.GetAsByteArray();
                    NetworkStream stream = Client.GetStream();
                    stream.Write(handshakeRequestByteArray, 0, handshakeRequestByteArray.Length);
                    stream.Read(buffer, 0, buffer.Length);

                    var handshakeResponse = PeerHandshake.ConvertFromResponse(buffer);
                    if (handshakeResponse.BittorrentProtocol == PeerHandshake.PSTR && handshakeResponse.InfoHash.ToLowerInvariant() == handshakeRequest.InfoHash.ToLowerInvariant())
                    {
                        Handshaked = true;
                    }
                    else
                    {
                        throw new Exception("Handshake response incorrect");
                    }

                    buffer = new byte[1024];
                    stream.Read(buffer, 0, buffer.Length);
                    if (buffer[4] == (int)MessageType.Bitfield)
                    {
                        var length = EndianBitConverter.Big.ToInt32(buffer.SubArray(0, 4), 0) - 1;
                        SetBitField(buffer.SubArray(5, length));
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private void SetBitField(byte[] array)
        {
            Bitfield = new BitArray(_Torrent.NumberOfPieces);
            //int offset = 0;
            for (var i = 0; i < array.Length; i++)
            {
                var sub = new BitArray(array.SubArray(i, 1));
                for (var j = 0; j < sub.Length; j++)
                {
                    Bitfield[(i * 8) + j] = sub[j];
                }
            }
        }

        public static byte[] EncodeRequest(int index, int begin, int length)
        {
            byte[] message = new byte[17];
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(13), 0, message, 0, 4);
            message[4] = (byte)MessageType.Request;
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(index), 0, message, 5, 4);
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(begin), 0, message, 9, 4);
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(length), 0, message, 13, 4);

            return message;
        }

        public static byte[] EncodeState(MessageType type)
        {
            byte[] message = new byte[5];
            Buffer.BlockCopy(EndianBitConverter.Big.GetBytes(1), 0, message, 0, 4);
            message[4] = (byte)type;
            return message;
        }

        public static bool DecodeBitfield(byte[] bytes, int pieces, out bool[] isPieceDownloaded)
        {
            isPieceDownloaded = new bool[pieces];

            int expectedLength = Convert.ToInt32(Math.Ceiling(pieces / 8.0)) + 1;

            if (bytes.Length != expectedLength + 4 || EndianBitConverter.Big.ToInt32(bytes, 0) != expectedLength)
            {
                //ConsoleUtil.WriteError("invalid bitfield, first byte must equal " + expectedLength);
                return false;
            }

            BitArray bitfield = new BitArray(bytes.Skip(5).ToArray());

            for (int i = 0; i < pieces; i++)
                isPieceDownloaded[i] = bitfield[bitfield.Length - 1 - i];

            return true;
        }

    }
}
