using BencodeNET.Torrents;

namespace torrent_library.Model
{
    public class FileDownloadInfo
    {
        public long Length { get; set; }
        public MultiFileInfo FileInfo { get; set; }
        public long FileOffset { get; set; }
    }
}