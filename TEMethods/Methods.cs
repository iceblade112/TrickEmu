using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu
{
    public class Methods
    {
        public static string sep(string str, string delim)
        {
            int len = str.IndexOf(delim);
            if (len > 0)
            {
                return str.Substring(0, len);
            }
            return "";
        }

        public static string getString(byte[] bytes, int i)
        {
            return Config.encoding.GetString(bytes);
        }

        /*public static void echoColor(string from, ConsoleColor color, string write)
        {
            echoColor(from, color, write, new string[] { });
        }

        public static void echoColor(string from, ConsoleColor color, string write, string[] args)
        {
            Console.ForegroundColor = color;
            Console.Write("[" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + " | " + from + "] ");
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i != args.Length; i++)
            {
                write = write.Replace("{" + i + "}", args[i]);
            }
            Console.WriteLine(write);
        }*/

        public static ushort ReadUshort(byte[] bytes, int pos)
        {
            byte[] ba = new byte[2] { bytes[pos], bytes[pos + 1] };
            return BitConverter.ToUInt16(ba.Reverse().ToArray(), 0);
        }

        public static string cleanString(string str)
        {
            return str.TrimEnd(new char[] { (char)0 }).Replace("\x00", "").Replace("\u0000", "");
        }

        // From http://stackoverflow.com/questions/8041343/how-to-split-a-byte
        // Author: driis http://stackoverflow.com/users/13627/driis
        public static IEnumerable<byte[]> Split(byte splitByte, byte[] buffer)
        {
            List<byte> bytes = new List<byte>();
            foreach (byte b in buffer)
            {
                if (b != splitByte)
                    bytes.Add(b);
                else
                {
                    yield return bytes.ToArray();
                    bytes.Clear();
                }
            }
            yield return bytes.ToArray();
        }
    }
}
