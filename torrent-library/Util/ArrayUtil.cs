using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Util
{
    public static class ArrayUtil
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static void ShiftLeft<T>(List<T> lst, int shifts)
        {
            for (int i = shifts; i < lst.Count; i++)
            {
                lst[i - shifts] = lst[i];
            }

            for (int i = lst.Count - shifts; i < lst.Count; i++)
            {
                lst[i] = default(T);
            }
        }

        public static void ShiftRight<T>(List<T> lst, int shifts)
        {
            for (int i = lst.Count - shifts - 1; i >= 0; i--)
            {
                lst[i + shifts] = lst[i];
            }

            for (int i = 0; i < shifts; i++)
            {
                lst[i] = default(T);
            }
        }
    }
}
