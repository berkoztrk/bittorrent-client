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

        public void WriteData(string path, byte[] bytes, long offset)
        {
            lock_.TryEnterWriteLock(int.MaxValue);
            try
            {
                using (FileStream fs = new FileStream(@"C:\torrents\" + path, FileMode.Append))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
            finally
            {
                lock_.ExitWriteLock();
            }
        }
    } 
}
