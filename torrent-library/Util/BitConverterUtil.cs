using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Util
{
    public static class BitConverterUtil
    {
        public static byte[] GetBytes(int obj)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.GetBytes(obj).Reverse().ToArray();
            }
            else
            {
                return BitConverter.GetBytes(obj);
            }
        }

        public static byte[] GetBytes(long obj)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.GetBytes(obj).Reverse().ToArray();
            }
            else
            {
                return BitConverter.GetBytes(obj);
            }
        }

        public static byte[] GetBytes(string obj)
        {
            {
                if (BitConverter.IsLittleEndian)
                {
                    return Encoding.Default.GetBytes(obj).Reverse().ToArray();
                }
                else
                {
                    return Encoding.Default.GetBytes(obj);
                }

            }
        }

        public static int ToInt(byte[] obj)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(obj.Reverse().ToArray(), 0);
            }
            else
            {
                return BitConverter.ToInt32(obj, 0);
            }
        }

        public static long ToLong(byte[] obj)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt64(obj.Reverse().ToArray(), 0);
            }
            else
            {
                return BitConverter.ToInt64(obj, 0);
            }
        }


    }
}
