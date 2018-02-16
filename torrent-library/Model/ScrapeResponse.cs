using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using torrent_library.Util;

namespace torrent_library.Model
{
    public class ScrapeResponse
    {
        public int ResponseAction { get; set; }
        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public int Completed { get; set; }
        public int ResponseLength { get; set; }
        public string ErrorMessage { get; set; }

        public ScrapeResponse(byte[] scrapeResponse)
        {

            var responseAction = scrapeResponse.SubArray(0, 4);
            var _action = BitConverterUtil.ToInt(responseAction);
            if (_action == TrackerAction.Error)
            {
                var errorMessage = BitConverterUtil.ToString(responseAction.SubArray(8, responseAction.Length - 8));
                ErrorMessage = errorMessage;
                throw new Exception(String.Format("I got error like this help!! => {0}", errorMessage));
            }

            var responseSeeders = scrapeResponse.SubArray(8, 4);
            var responseCompleted = scrapeResponse.SubArray(12, 4);
            var responseLeechers = scrapeResponse.SubArray(16, 4);
            Seeders = BitConverterUtil.ToInt(responseSeeders);
            Completed = BitConverterUtil.ToInt(responseCompleted);
            Leechers = BitConverterUtil.ToInt(responseLeechers);
            ResponseLength = scrapeResponse.Length;


        }
    }
}
