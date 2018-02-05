﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

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
    }
}
