using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace bittorrent_client
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public override void EndInit()
        {
            base.EndInit();
          
            TorrentFile torrent = BU.Decode(.DecodeTorrentFile("Ubuntu.torrent");

            // Calculate info hash (e.g. "B415C913643E5FF49FE37D304BBB5E6E11AD5101")
            string infoHash = torrent.CalculateInfoHash();

            // Get name and size of each file in 'files' list of 'info' dictionary ("multi-file mode")
            BList files = (BList)torrent.Info["files"];
            foreach (BDictionary file in files)
            {
                // File size in bytes (BNumber has implicit conversion to int and long)
                int size = (BNumber)file["length"];

                // List of all parts of the file path. 'dir1/dir2/file.ext' => dir1, dir2 and file.ext
                BList path = (BList)file["path"];

                // Last element is the file name
                BString fileName = (BString)path.Last();

                // Converts fileName (BString = bytes) to a string
                string fileNameString = fileName.ToString(Encoding.UTF8);
            }
        }
    }
}
