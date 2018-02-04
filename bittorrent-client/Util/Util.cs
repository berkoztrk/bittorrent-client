using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace bittorrent_client.Util
{
    public static class Util
    {
        public static class AppSettings
        {
            public static string TargetPath
            {
                get { return GetAppSettingsValue("TargetPath"); }
            }
        }

        public static string GetAppSettingsValue(string key)
        {
            return ConfigurationManager.AppSettings[key] as string;
        }
    }
}
