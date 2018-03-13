using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace torrent_library.Util
{
    public class TorrentFileWriter
    {
        private ReaderWriterLockSlim lock_ = new ReaderWriterLockSlim();

        public void WriteData(string path, byte[] bytes, long fileSize, long offset)
        {

            try
            {
                using (var fs = File.Open(@"C:\torrents\" + path, FileMode.Open))
                {
                    fs.SetLength(fileSize);
                    fs.Position = offset;
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (FileNotFoundException e)
            {
                using (var fs = File.Open(@"C:\torrents\" + path, FileMode.Create))
                {
                    fs.SetLength(fileSize);
                    fs.MakeSparse();
                    fs.SetSparseRange(0, fs.Length);
                    fs.Position = offset;
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            catch (IOException e)
            {
                WriteData(path, bytes, fileSize, offset);
            }
        }

    }
}

