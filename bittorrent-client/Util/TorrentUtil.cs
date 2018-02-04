using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoTorrent.Client;
using MonoTorrent.Common;
using MonoTorrent;
using MonoTorrent.Client.Encryption;
using System.IO;
using System.Net;

namespace bittorrent_client.Util
{
    public static class TorrentUtil
    {


        public static void DownloadTorrent(string magnetLink)
        {
            MagnetLink link = new MagnetLink(magnetLink);

            //URL stores the magnetlink
            EngineSettings settings = new EngineSettings();
            settings.AllowedEncryption = EncryptionTypes.All;
            settings.SavePath = Path.Combine(Environment.CurrentDirectory, "Downloads");

            string torrentFilePath = Path.Combine(Environment.CurrentDirectory, "TorrentFiles");

            if (!Directory.Exists(settings.SavePath))
                Directory.CreateDirectory(settings.SavePath);

            if (!Directory.Exists(torrentFilePath))
                Directory.CreateDirectory(torrentFilePath);

            //Create a new engine, give it some settings and use it.
            ClientEngine engine = new ClientEngine(settings);
            engine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, 6969));

            TorrentManager manager = new TorrentManager(link, engine.Settings.SavePath, new TorrentSettings(), torrentFilePath);
            engine.Register(manager);
            manager.Start();


            Console.ReadLine();
        }
    }
}
 