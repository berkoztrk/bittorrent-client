using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library_test
{
    public class Logger
    {


        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Logger Instance { get; set; }

        private Logger() { }

        public Logger GetInstance()
        {
            if (Instance == null)
                Instance = new Logger();

            return Instance;
        }
    }

}