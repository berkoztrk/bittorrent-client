using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace torrent_library
{
    public class Settings
    {
        public string DownloadPath { get; set; }

        public Settings() { }

        public static Settings Load()
        {
            try
            {
                return LoadSettingsFromFile();
            }
            catch (FileNotFoundException fnfe)
            {
                var settings = new Settings()
                {
                    DownloadPath = @"C:/torrents"
                };
                settings.Save();
                return settings;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Save()
        {
            var jss = new JavaScriptSerializer();
            if (!File.Exists("settings.json"))
            {
                File.Create("settings.json");
            }

            File.WriteAllText("settings.json", jss.Serialize(this));
        }

        private static Settings LoadSettingsFromFile()
        {
            var settings = File.ReadAllText("settings.json");
            var jss = new JavaScriptSerializer();
            return jss.Deserialize<Settings>(settings);
        }
    }
}
