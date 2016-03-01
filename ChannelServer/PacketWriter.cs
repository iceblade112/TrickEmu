using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace TrickEmu
{
    class PacketWriter
    {
        public static void ReqCharSelect(byte[] dec, Socket sock)
        {
            // Char select menu packet

            byte[][] udetail = Methods.Split(0x00, dec).ToArray();

            if(udetail.Length < 3)
            {
                // Invalid packet data
                return;
            }

            Console.WriteLine("User ID: " + Config.encoding.GetString(udetail[0]));
            Console.WriteLine("User PW: " + Config.encoding.GetString(udetail[1]));
            Console.WriteLine("Client Version: " + Config.encoding.GetString(udetail[2]));

            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("D2 07 00 00 01");
            data.WriteHexString("01 D8 1D F6 05 00 00 00 00 0B E1 F5 05 00 00 00 00 00"); // Unknown
            data.WriteString("HI ERIC", 16); // 16 byte character name
            data.WriteHexString("00 00 00 00 01 00 01 00 04 02 01 03 9C 01 9C 01 98 01 CC 00 CC 00 CC 00 66 00 66 00 66 00 32 01 32 01 32 01 90 01 00 00 00 00 00 00 00 00"); // Unknown
            data.WriteByte(5); // 1 byte (int) level
            data.WriteHexString("00 00 00 21 00 B1 0D 00 00 D4 0A 00 00 21 00"); // Unknown
            data.WriteUInt32(290); // galders, 4 bytes (uint32)
            // TO-DO: Normal short write
            data.WriteHexString("60 09"); // Current HP
            data.WriteHexString("4A 06"); // Current MP
            data.WriteHexString("14 00 00 00 00 00 00 00 04 00 00 00 06 00 00 00 D7 A7 E9 01 40 31 8C E3 79 72 D1 01 40 31 8C E3 79 72 D1 01 65 35 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ED D1 8C 77 00 00 00 00 EE D1 8C 77 00 00 00 00 EC D1 8C 77 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 D8 1D F6 05 00 00 00 00 60 00 00 00 F4 1A 26 1B"); // Everything else, unknown
            
            byte[] msg = data.getPacket();
            sock.Send(msg);
        }

        public static void SelectCharIngame(byte[] dec, Socket sock)
        {
            // Char select (to go ingame)
            byte[] msg = new byte[] { 0x2D, 0x00, 0x00, 0x00, 0xDE, 0x07, 0x00, 0x00, 0x00, 0xD8, 0x1D, 0xF6, 0x05, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x3C, 0x15, 0x00, 0x00, 0x31, 0x32, 0x37, 0x2E, 0x30, 0x2E, 0x30, 0x2E, 0x31, 0x00, 0xF6, 0x55, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x86, 0x15, 0x00, 0x00, 0x00 };
            sock.Send(msg);
            
            byte[] msg2 = new byte[] { 0x13, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0xE1, 0xF5, 0x05, 0x00, 0x00, 0x00, 0x00 };
            sock.Send(msg2);
        }
    }
}
