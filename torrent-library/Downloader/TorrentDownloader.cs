using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Model;
using torrent_library.Util;

namespace torrent_library.Downloader
{
    public class TorrentDownloader
    {

        public AnnounceResponse _AnnounceResponse { get; set; }

        public TorrentDownloader(AnnounceResponse announceResponse)
        {
            _AnnounceResponse = announceResponse;
        }



        public TorrentDownloader() { }

        //public Task<string> DownloadAsync()
        //{
        //    HttpClient client = new HttpClient();
        //    var responseMessage = client.GetAsync(string.Format("http://{0}:{1}", IP, Port));
        //    var responseMessageContent = responseMessage.Result.Content.ReadAsStringAsync();
        //    return responseMessageContent;
        //}

        public int Download()
        {
            for (int i = 0; i < _AnnounceResponse.IPAddresses.Count; i++)
            {
                try
                {
                    Socket s = new Socket(SocketType.Dgram, ProtocolType.Udp);
                    s.ReceiveTimeout = 15000;

                    byte[] buffer = new byte[1024];

                    s.Connect(IPAddress.Parse(BitConverterUtil.IntToIP(_AnnounceResponse.IPAddresses[i])),
                        Convert.ToInt32(_AnnounceResponse.Ports[i]));
                    s.Send(buffer);
                    Console.WriteLine(string.Format("IsConnected : {0}. IsBlocking: {1}, Available : {2}", s.Connected, s.Blocking, s.Available));
                    if (s.Available > 0)
                    {
                        int resp = s.Receive(buffer, SocketFlags.None);
                        return resp;
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            return 0;
        }

    }
}
