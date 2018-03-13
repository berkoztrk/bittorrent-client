using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library;

namespace bittorrent_client.BO
{
    public class SqTorrentContext
    {
        public event Action<object, TorrentProcessor> ProcessorsUpdated;

        private static SqTorrentContext Instance
        {
            get; set;
        }
        public ConcurrentDictionary<string, TorrentProcessor> Processors { get; set; }

        private SqTorrentContext()
        {
            Processors = new ConcurrentDictionary<string, TorrentProcessor>();
        }

        public static SqTorrentContext GetInstance()
        {
            if (Instance == null)
                Instance = new SqTorrentContext();
            return Instance;
        }

        public void AddProcessor(TorrentProcessor processor)
        {
            Processors.TryAdd(processor.Manager.Torrent.OriginalInfoHash, processor);

            if (ProcessorsUpdated != null)
                ProcessorsUpdated(this, processor);

        }

    }
}
