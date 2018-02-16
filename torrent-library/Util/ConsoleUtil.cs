using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace torrent_library.Util
{
    public static class ConsoleUtil
    {
        public static void WriteError(string msg, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg, args);
        }

        public static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
        }

        public static void WriteSuccess(string msg, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg, args);
        }

        public static void WriteSuccess(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
        }

        public static void Write(string msg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(msg);
        }

        public static void Write(string msg, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(msg, args);
        }
    }
}
