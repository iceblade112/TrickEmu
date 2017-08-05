using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TELib
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
            return Encoding.GetEncoding("gb2312").GetString(bytes);
        }

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
