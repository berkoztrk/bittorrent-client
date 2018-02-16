using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using torrent_library.Model;
using torrent_library.Util;

namespace torrent_library.Downloader
{
    public class TorrentDownloader
    {

        public AnnounceResponse _AnnounceResponse { get; set; }
        public AnnounceRequest _AnnounceRequest { get; set; }
        public Torrent _Torrent { get; set; }

        public TorrentDownloader(AnnounceRequest announceRequest, AnnounceResponse announceResponse, Torrent torrent)
        {
            _AnnounceRequest = announceRequest;
            _AnnounceResponse = announceResponse;
            _Torrent = torrent;
        }

        public TorrentDownloader() { }

        public int StartDownload()
        {
            ConsoleUtil.Write("Connecting to peers...");
            for (int i = 0; i < _AnnounceResponse.IPPort.Count; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(Download), _AnnounceResponse.IPPort[i]);
            }
            return 0;
        }


        private void Download(object ipPort)
        {
            var tcpClient = new TcpClient();
            tcpClient.ReceiveTimeout = 2000;

            try
            {
                var IPPort = ipPort as IPPortPair;
                var IP = IPPort.IP;
                var Port = IPPort.Port;
                tcpClient.Connect(IP, Port);
                ConsoleUtil.WriteSuccess("Connected to peer. IP : {0}, Port : {1}", IP.ToString(), Port);
                do
                {
                    byte[] buffer = new byte[1024];
                    if (tcpClient.Connected)
                    {

                        var stream = tcpClient.GetStream();
                        var downloadHandshakeRequest = _AnnounceRequest.GetDownloadHandshakeRequest();
                        stream.Write(downloadHandshakeRequest, 0, downloadHandshakeRequest.Length);
                        if (stream.DataAvailable)
                        {
                            stream.Read(buffer, 0, buffer.Length);
                            ConsoleUtil.WriteSuccess("Received {0} not empty data", buffer.Where(x => x != 0).Count());
                        }
                    }
                } while (tcpClient.Connected);

            }
            catch (Exception e)
            {
                ConsoleUtil.WriteError(e.Message);
                tcpClient.Dispose();
            }
        }

    }
}
