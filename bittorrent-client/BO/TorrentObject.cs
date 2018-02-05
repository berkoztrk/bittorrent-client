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
using log4net;

namespace bittorrent_client.BO
{
    public class TorrentObject
    {
        private ILog Logger;

        public TorrentObject()
        {
            Logger = LogManager.GetLogger(typeof(TorrentObject));
        }

        public void SaveTorrentFileFromMagnet(string magnetLink)
        {
            //MagnetLink link = new MagnetLink(magnetLink);
            var hash = InfoHash.FromMagnetLink(magnetLink);
            MagnetLink link = new MagnetLink(magnetLink);
            try
            {
                File.WriteAllBytes(FileUtil.ConvertTorrentFileName(link.Name), hash.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Info(ex.ToString());
            }

        }

        public static void DownloadTorrent(string magnetLink)
        {
            MagnetLink link = new MagnetLink(magnetLink);

            //URL stores the magnetlink
            //EngineSettings settings = new EngineSettings();
            //settings.AllowedEncryption = EncryptionTypes.All;
            //settings.SavePath = Path.Combine(Environment.CurrentDirectory, "Downloads");

            //string torrentDirPath = Path.Combine(Environment.CurrentDirectory, @"TorrentFiles");

            //if (!Directory.Exists(settings.SavePath))
            //    Directory.CreateDirectory(settings.SavePath);

            //if (!Directory.Exists(torrentDirPath))
            //    Directory.CreateDirectory(torrentDirPath);

            //ClientEngine engine = new ClientEngine(settings);
            //engine.ChangeListenEndpoint(new IPEndPoint(IPAddress.Any, 6969));

            //TorrentSettings torrentDefaults = new TorrentSettings(4, 150, 0, 0);

            //TorrentManager manager = new TorrentManager(link, engine.Settings.SavePath, torrentDefaults, torrentDirPath);
            //engine.Register(manager);
            //manager.Start();
        }

        private static void Manager_TorrentStateChanged(object sender, TorrentStateChangedEventArgs e)
        {

        }
    }
}
