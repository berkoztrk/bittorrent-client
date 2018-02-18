using System;
using System.Collections.Generic;
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
        public IPPortPair _IPPortPair { get; set; }
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public bool Handshaked { get; set; }

        public Peer(IPPortPair pair)
        {
            this._IPPortPair = pair;
            IP = pair.IP;
            Port = pair.Port;
            Handshaked = false;
        }

        private bool ConnectToPeer(ref TcpClient tcpClient)
        {
            try
            {
                var result = tcpClient.BeginConnect(IP, Port, null, null);
                var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(2));
                if (success)
                {
                    ConsoleUtil.WriteSuccess("Connected to peer. IP : {0}, Port : {1}", IP.ToString(), Port);
                    return true;
                }
                else
                    throw new Exception("Peer connection timed out.");
            }
            catch (Exception e)
            {
                ConsoleUtil.WriteError("Could not connect to peer. IP : {0}, Port : {1} => {2}", IP.ToString(), Port, e.Message);
                return false;
            }
        }

        public bool Connect()
        {
            var tcpClient = new TcpClient();

            try
            {
                return ConnectToPeer(ref tcpClient);
            }
            catch (Exception)
            {
                return false;
            }
            finally { tcpClient.Dispose(); }
        }

        public void HandShake(PeerHandshake handshakeRequest)
        {
            var tcpClient = new TcpClient();
            try
            {
                bool connected = ConnectToPeer(ref tcpClient);
                if (connected)
                {
                    byte[] buffer = new byte[68];
                    var handshakeRequestByteArray = handshakeRequest.GetAsByteArray();
                    NetworkStream stream = tcpClient.GetStream();
                    stream.Write(handshakeRequestByteArray, 0, handshakeRequestByteArray.Length);
                    stream.Read(buffer, 0, buffer.Length);

                    var handshakeResponse = PeerHandshake.ConvertFromResponse(buffer);
                    if (handshakeResponse.BittorrentProtocol == PeerHandshake.PSTR && handshakeResponse.InfoHash.ToLowerInvariant() == handshakeRequest.InfoHash.ToLowerInvariant())
                    {
                        Handshaked = true;
                        ConsoleUtil.WriteSuccess("Handshaked successfully with peer {0}:{1}", IP.ToString(), Port);
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleUtil.WriteError("Handshake message couldnt sent successfully to peer {0}:{1},{2}", IP.ToString(), Port, e.Message);
            }
            finally
            {
                tcpClient.Dispose();
            }
        }

        public void GetHandshake(PeerHandshake handshakeRequest)
        {
            var tcpClient = new TcpClient();
            try
            {
                bool connected = ConnectToPeer(ref tcpClient);
                if (connected)
                {
                    var handshakeRequestByteArray = handshakeRequest.GetAsByteArray();
                    tcpClient.GetStream().Write(handshakeRequestByteArray, 0, handshakeRequestByteArray.Length);
                    ConsoleUtil.WriteSuccess("Handshake message sended successfully to peer {0}:{1}", _IPPortPair.IP.ToString(), _IPPortPair.Port);
                    tcpClient.Close();
                }
            }
            catch (Exception e)
            {
                ConsoleUtil.WriteError("Handshake message couldnt sent successfully to peer {0}:{1},{2}", _IPPortPair.IP.ToString(), _IPPortPair.Port, e.Message);
            }
            finally
            {
                tcpClient.Dispose();
            }
        }

    }
}
