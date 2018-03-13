using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;
using static System.Net.Mime.MediaTypeNames;

namespace torrent_library.MagnetUtils
{
    public class MagnetToTorrent
    {
        private const int TIMEOUT = 10; // seconds
        public string InfoHash { get; set; }
        public bool DownloadCompleted { get; set; }
        private static readonly string[] Services = new string[]
        {
            @"http://itorrents.org/torrent/{0}.torrent",
            @"http://archive.org/download/{0}/{0}_archive.torrent"
        };

        public MagnetToTorrent(string infoHash)
        {
            this.InfoHash = infoHash;
            this.DownloadCompleted = false;
        }

        public string Download()
        {
            string fileName = InfoHash + ".torrent";
            var savePath = @"C:\torrents\" + fileName;

            var started = DateTime.Now;
            DownloadTorrentFile(fileName, savePath);
            if (DownloadCompleted)
                return savePath;
            return null;
        }

        private void DownloadTorrentFile(string fileName, string savePath)
        {
            foreach (var service in Services)
            {
                try
                {
                    string remoteUri = string.Format(service,
                       InfoHash.ToUpperInvariant());

                    if (File.Exists(savePath))
                    {
                        ConsoleUtil.Write("File exists => " + savePath);
                        DownloadCompleted = true;
                        return;
                    }
                    ConsoleUtil.Write("File does not exists, trying to download torrent file from => " + remoteUri);

                    HttpWebRequest httpRequest = (HttpWebRequest)
WebRequest.Create(remoteUri);
                    httpRequest.Method = WebRequestMethods.Http.Get;
                    HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    Stream httpResponseStream = httpResponse.GetResponseStream();

                    // create and open a FileStream, using calls dispose when done

                    if (httpResponse.ContentType == "application/x-bittorrent")
                    {
                        using (var fs = File.Create(savePath))
                        {
                            // Copy all bytes from the responsestream to the filestream
                            httpResponseStream.CopyTo(fs);
                        }
                    }
                       
                    //WebClient myWebClient = new WebClient();
                    //myStringWebResource = remoteUri;

                    //myWebClient.DownloadFileAsync(new Uri(myStringWebResource), savePath);
                }
                catch (Exception e)
                {
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                    }
                    ConsoleUtil.WriteError(e.Message);
                }
            }

        }

    }
}
