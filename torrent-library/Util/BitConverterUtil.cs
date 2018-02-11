using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
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

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return BitConverter.IsLittleEndian ? bytes.Reverse().ToArray() : bytes;
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

        public static ushort ToShortInt(byte[] obj)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToUInt16(obj.Reverse().ToArray(), 0);
            }
            else
            {
                return BitConverter.ToUInt16(obj, 0);
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

        public static string ToString(byte[] obj)
        {
            if (BitConverter.IsLittleEndian)
            {
                return Encoding.Default.GetString(obj.Reverse().ToArray());
            }
            else
            {
                return Encoding.Default.GetString(obj);
            }
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] HexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < HexAsBytes.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                HexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return HexAsBytes;
        }

        public static string IntToIP(int IP)
        {
            var byteArray = BitConverter.IsLittleEndian ? BitConverter.GetBytes(IP).Reverse().ToArray() : BitConverter.GetBytes(IP);
            string ipAddress = new IPAddress(byteArray).ToString();
            return ipAddress;
        }
    }
}
