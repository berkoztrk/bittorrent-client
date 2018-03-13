using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Windows;

namespace bittorrent_client
{
    public static class Util
    {
        public static class AppSettings
        {
            public static string TorrentFilePath
            {
                get { return Path.Combine(Environment.CurrentDirectory, GetAppSettingsValue("TorrentFilePath")); }
            }

            public static string DownloadPath
            {
                get { return Path.Combine(Environment.CurrentDirectory, GetAppSettingsValue("DownloadPath")); }
            }
        }

        public static string GetAppSettingsValue(string key)
        {
            return ConfigurationManager.AppSettings[key] as string;
        }

        public static void AddOnUI<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        internal static string GetDownloadSpeed(double dLSpeed)
        {
            if (dLSpeed > 1000000)
            {
                return Math.Ceiling(dLSpeed / 1000000) + "Mb/s";
            }
            else
                return Math.Ceiling(dLSpeed / 1000) + "Kb/s";
        }
    }
}
