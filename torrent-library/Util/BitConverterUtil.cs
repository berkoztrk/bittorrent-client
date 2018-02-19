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
            return EndianBitConverter.Big.GetBytes(obj);
        }

        public static byte[] GetBytes(long obj)
        {
            return EndianBitConverter.Big.GetBytes(obj);
        }

        public static byte[] FromUInt(ushort length)
        {
            return EndianBitConverter.Big.GetBytes(length);
        }

        public static byte[] FromChar(char c)
        {
            return EndianBitConverter.Big.GetBytes(c);
        }

        public static int ToInt(byte[] obj)
        {
            return EndianBitConverter.Big.ToInt32(obj);
        }

        public static ushort ToUShortInt(byte[] obj)
        {
            return EndianBitConverter.Big.ToUInt16(obj);
        }

        public static long ToLong(byte[] obj)
        {
            return EndianBitConverter.Big.ToInt64(obj);
        }

        public static char ToChar(byte[] v)
        {
            return EndianBitConverter.Big.ToChar(v);
        }

        public static string ToString(byte[] obj)
        {
            return Encoding.UTF8.GetString(obj);
        }

        public static byte[] FromHexString(string hexString)
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

        public static byte[] FromString(string obj)
        {
            return Encoding.UTF8.GetBytes(obj);
        }

        public static string ByteArrayToHexString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

    }
}
