using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu
{
    /// <summary>
    /// Chinese packet buffer knockoff
    /// Literally.
    /// </summary>
    public class PacketBuffer
    {
        private List<byte> header;
        private List<byte> packet;

        public PacketBuffer()
        {
            header = new List<byte>();
            packet = new List<byte>();
        }

        /// <summary>
        /// Returns the packet payload as a byte array
        /// </summary>
        public byte[] getPacket(bool encrypt = true)
        {
            /*if(encrypt)
            {
                return Encryption.encrypt(packet.ToArray());
            }*/
            if(header.ToArray().Length > 0)
            {
                // Create packet using Encryption class
                return Encryption.encrypt(header.ToArray(), packet.ToArray());
            }
            // Header is empty; probably already existing data
            return packet.ToArray();
        }

        /// <summary>
        /// Returns the packet without encryption as a byte array
        /// Used for server select, etc?
        /// </summary>
        public byte[] getPacketDecrypted()
        {
            ushort len = (ushort)(header.ToArray().Length + packet.ToArray().Length + 4);
            
            var send = new List<byte>();

            // Length
            foreach (byte b in BitConverter.GetBytes(len))
            {
                send.Add(b);
            }

            send.Add(0x00);
            send.Add(0x00); // No CRC

            // Packet header
            foreach (byte b in header.ToArray())
            {
                send.Add(b);
            }

            // Packet payload
            foreach (byte b in packet.ToArray())
            {
                send.Add(b);
            }

            return send.ToArray();
        }

        /// <summary>
        /// Inserts a byte to the specified position of the packet "buffer"
        /// </summary>
        /// <param name="b">The byte to insert</param>
        public void WriteByteInsert(int pos, byte b)
        {
            header.Insert(pos, b);
            return;
        }

        /// <summary>
        /// Writes a ushort to the packet header
        /// </summary>
        /// <param name="head">The ushort to write</param>
        public void WriteHeaderUshort(ushort head)
        {
            header.AddRange(BitConverter.GetBytes(head));
            return;
        }

        /// <summary>
        /// Writes a byte to the packet header
        /// </summary>
        /// <param name="head"></param>
        public void WriteHeaderByte(byte head)
        {
            header.Add(head);
            return;
        }

        /// <summary>
        /// Writes a short to the packet header
        /// </summary>
        /// <param name="s">The short to write</param>
        public void WriteShort(short s)
        {
            header.AddRange(BitConverter.GetBytes(s));
            return;
        }

        /// <summary>
        /// Writes a byte array to the packet header
        /// </summary>
        /// <param name="head"></param>
        public void WriteHeaderByteArray(byte[] head)
        {
            header.AddRange(head);
            return;
        }

        /// <summary>
        /// Writes a hex string to the packet header
        /// </summary>
        /// <param name="str"></param>
        public void WriteHeaderHexString(string str)
        {
            header.AddRange(StringToByteArray(str.Replace(" ", "")));
            return;
        }

        /// <summary>
        /// Clears the packet header data
        /// </summary>
        public void ClearHeader()
        {
            header.Clear();
            return;
        }

        /// <summary>
        /// Writes a byte to the packet "buffer"
        /// </summary>
        /// <param name="b">The byte to write</param>
        public void WriteByte(byte b)
        {
            packet.Add(b);
            return;
        }

        /// <summary>
        /// Writes a byte array to the packet "buffer"
        /// </summary>
        /// <param name="b">The byte array to write</param>
        public void WriteByteArray(byte[] b)
        {
            packet.AddRange(b);
            return;
        }

        /// <summary>
        /// Writes a UInt32 to the packet "buffer"
        /// </summary>
        /// <param name="i">The Uint32 to write</param>
        public void WriteUInt32(uint i)
        {
            packet.AddRange(BitConverter.GetBytes(i));
            return;
        }

        /// <summary>
        /// Writes a Int32 to the packet "buffer"
        /// </summary>
        /// <param name="i">The int to write</param>
        public void WriteInt32(int i)
        {
            packet.AddRange(BitConverter.GetBytes(i));
            return;
        }

        /// <summary>
        /// Writes a byte n times to the packet "buffer"
        /// </summary>
        /// <param name="b">The byte to write n times</param>
        /// <param name="n">The amount of times to repeat-write b</param>
        public void WriteBytePad(byte b, int n)
        {
            List<byte> _temp = new List<byte>();
            for(int i = 1; i != n; i++)
            {
                _temp.Add(b);
            }
            packet.AddRange(_temp.ToArray());
            return;
        }

        /// <summary>
        /// Writes a ushort (unsigned Int16) to the packet "buffer"
        /// </summary>
        /// <param name="us">The ushort to write</param>
        public void WriteUshort(ushort us)
        {
            packet.AddRange(BitConverter.GetBytes(us));
            return;
        }

        /// <summary>
        /// Writes a hex string to the packet "buffer"
        /// </summary>
        /// <param name="str">The hex string to write</param>
        public void WriteHexString(string str)
        {
            packet.AddRange(StringToByteArray(str.Replace(" ", "")));
            return;
        }

        /// <summary>
        /// Writes a string to the packet "buffer"
        /// </summary>
        /// <param name="str">The string to write</param>
        public void WriteString(string str)
        {
            packet.AddRange(Config.encoding.GetBytes(str));
            return;
        }

        /// <summary>
        /// Writes a string to the packet "buffer" with a fixed length
        /// If the string is smaller than the length, 0x00s will be written
        /// </summary>
        /// <param name="str">The string to write</param>
        /// <param name="str">The length of the string</param>
        public void WriteString(string str, int len)
        {
            WriteByteArray(Config.encoding.GetBytes(str));
            if(str.Length < len)
            {
                for (int i = str.Length; i != len; i++) WriteByte(0x00);
            }
            return;
        }

        /// <summary>
        /// Converts a hex string to a byte array
        /// http://stackoverflow.com/a/321404/1908515
        /// Author: JaredPar http://stackoverflow.com/users/23283/jaredpar (and editors)
        /// </summary>
        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
