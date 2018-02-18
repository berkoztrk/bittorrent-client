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
        public string InfoHash { get; set; }
        private static readonly string[] Services = new string[]
        {
            @"http://itorrents.org/torrent/{0}.torrent"
        };

        public MagnetToTorrent(string infoHash)
        {
            this.InfoHash = infoHash;
        }

        public string DownloadTorrentFile()
        {
            foreach (var service in Services)
            {

                try
                {
                    string remoteUri = string.Format(service, InfoHash);
                    string fileName = InfoHash + ".torrent", myStringWebResource = null;
                    var savePath = @"C:\torrents\" + fileName;
              
                   
                    if (File.Exists(savePath))
                    {
                        ConsoleUtil.Write("File exists => " + savePath);
                        return savePath;
                    }


                    ConsoleUtil.Write("File does not exists, trying to download torrent file from => " + remoteUri);
                    WebClient myWebClient = new WebClient();
                    myStringWebResource = remoteUri + fileName;
                    var x = myWebClient.ResponseHeaders;
                    myWebClient.DownloadFile(myStringWebResource, savePath);
                    ConsoleUtil.WriteSuccess("Downloaded {0}.torrent file successfully", InfoHash);
                    return savePath;
                }
                catch (Exception e)
                {
                    ConsoleUtil.WriteError(e.Message); 
                }
            }
            return null;
        }

    }
}
