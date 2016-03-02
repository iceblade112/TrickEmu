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
        public static void CreateCharacter(byte[] dec, Socket sock)
        {
            if(!Program._clientSocketIdentifiers.ContainsKey(sock.GetHashCode()))
            {
                Console.WriteLine(sock.GetHashCode() + " does not exist in the identifier dictionary!");
                return;
            }

            string uid = Program._clientSocketIdentifiers[sock.GetHashCode()];
            int nochars = 0;

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM characters WHERE user = @userid;";
                    cmd.Parameters.AddWithValue("@userid", uid);
                    nochars = Convert.ToInt32(cmd.ExecuteScalar());
                    if (nochars >= 3)
                    {
                        Console.WriteLine("This guy already has 3+ characters!");
                        cmd.Dispose();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database error: " + ex);
                return;
            }

            //byte[] noheader = new byte[] { };
            //dec.CopyTo(noheader, 16);
            //Console.WriteLine(BitConverter.ToString(dec).Replace("-", " "));
            byte[][] data = Methods.Split(0x00, dec).ToArray();

            string newchar = Config.encoding.GetString(data[0]);
            byte[] charpoints = data[3];

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO characters (user, name) VALUES (@userid, @charname);";
                    cmd.Parameters.AddWithValue("@userid", uid);
                    cmd.Parameters.AddWithValue("@charname", newchar);
                    cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database error: " + ex);
                return;
            }

            PacketBuffer pdata = new PacketBuffer();
            pdata.WriteHeaderHexString("D8 07 00 00 01");
            pdata.WriteHexString("DF 1D F6 05 00 00 00 00 00 00 00 00 00 00 00 00 02"); // maybe DE 1D?
            pdata.WriteString(newchar, 16);
            pdata.WriteHexString("00 00 00 00 01 00 01 00 04 02 01 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 21 00 00 00 00 00 00 00 00 00 21 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

            sock.Send(pdata.getPacket());
        }

        public static void ReqCharSelect(byte[] dec, Socket sock)
        {
            // Char select menu packet

            byte[][] udetail = Methods.Split(0x00, dec).ToArray();

            if(udetail.Length < 3)
            {
                // Invalid packet data
                return;
            }

            string uid = Config.encoding.GetString(udetail[0]);

            if (Program._clientSocketIdentifiers.ContainsKey(sock.GetHashCode()))
            {
                Program._clientSocketIdentifiers.Remove(sock.GetHashCode());
            }
            Program._clientSocketIdentifiers.Add(sock.GetHashCode(), uid);
            
            Console.WriteLine("User ID: " + uid);
            Console.WriteLine("User PW: " + Config.encoding.GetString(udetail[1]));
            Console.WriteLine("Client Version: " + Config.encoding.GetString(udetail[2]));

            /*PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("D2 07 00 00 01");
            data.WriteByte(1); // Amount of characters
            data.WriteHexString("D8 1D F6 05 00 00 00 00 0B E1 F5 05 00 00 00 00 00"); // Unknown
            data.WriteString("Test", 16); // 16 byte character name
            data.WriteHexString("00 00 00 00 01 00 01 00 04 02 01 03 9C 01 9C 01 98 01 CC 00 CC 00 CC 00 66 00 66 00 66 00 32 01 32 01 32 01 90 01 00 00 00 00 00 00 00 00"); // Unknown
            data.WriteByte(5); // 1 byte (int) level
            data.WriteHexString("00 00 00 21 00 B1 0D 00 00 D4 0A 00 00 21 00"); // Unknown
            data.WriteUInt32(290); // galders, 4 bytes (uint32)
            // TO-DO: Normal short write
            data.WriteHexString("60 09"); // Current HP
            data.WriteHexString("4A 06"); // Current MP
            data.WriteHexString("14 00 00 00 00 00 00 00 04 00 00 00 06 00 00 00 D7 A7 E9 01 40 31 8C E3 79 72 D1 01 40 31 8C E3 79 72 D1 01 65 35 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ED D1 8C 77 00 00 00 00 EE D1 8C 77 00 00 00 00 EC D1 8C 77 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

            //data.WriteHexString("DB 1D F6 05 00 00 00 00 0B E1 F5 05 00 00 00 00 01 43 68 61 72 32 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 01 00 04 02 01 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 21 00 00 00 00 00 00 00 00 00 21 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 F0 A0 E2 6A E4 73 D1 01 F0 A0 E2 6A E4 73 D1 01 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"); // Second character

            data.WriteByte(1); // Amount of characters again?
            data.WriteHexString("D8 1D F6 05 00 00 00 00 60 00 00 00 F4 1A 26 1B"); // Everything else, unknown
            */

            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("D2 07 00 00 01");
            /*data.WriteByte(2); // Amount of characters
            data.WriteHexString("D8 1D F6 05 00 00 00 00 0B E1 F5 05 00 00 00 00"); // Unknown
            data.WriteByte(0x00); // Card position (0 index)
            data.WriteString("Test", 16); // 16 byte character name
            data.WriteHexString("00 00 00 00 01 00 01 00 04 02 01 03 9C 01 9C 01 98 01 CC 00 CC 00 CC 00 66 00 66 00 66 00 32 01 32 01 32 01 90 01 00 00 00 00 00 00 00 00"); // Unknown
            data.WriteByte(5); // 1 byte (int) level
            data.WriteHexString("00 00 00 21 00 B1 0D 00 00 D4 0A 00 00 21 00"); // Unknown
            data.WriteUInt32(290); // galders, 4 bytes (uint32)
            // TO-DO: Normal short write
            data.WriteHexString("60 09"); // Current HP
            data.WriteHexString("4A 06"); // Current MP
            data.WriteHexString("14 00 00 00 00 00 00 00 04 00 00 00 06 00 00 00 D7 A7 E9 01 40 31 8C E3 79 72 D1 01 40 31 8C E3 79 72 D1 01 65 35 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ED D1 8C 77 00 00 00 00 EE D1 8C 77 00 00 00 00 EC D1 8C 77 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

            // 2nd char
            data.WriteHexString("DB 1D F6 05 00 00 00 00 0B E1 F5 05 00 00 00 00");
            data.WriteByte(0x01); // Card position (0 index)
            data.WriteString("Test2", 16); // 16 byte character name
            data.WriteHexString("00 00 00 00 01 00 01 00 04 02 01 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"); // ??
            data.WriteByte(16); // Level
            data.WriteHexString("00 00 00 21 00 00 00 00 00 00 00 00 00 21 00");
            data.WriteUInt32(29999); // Current galders
            data.WriteUshort(50); // HP
            data.WriteUshort(50); // MP
            data.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 F0 A0 E2 6A E4 73 D1 01 F0 A0 E2 6A E4 73 D1 01 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

            data.WriteByte(2); // Amount of characters again?
            data.WriteHexString("D8 1D F6 05 00 00 00 00 60 00 00 00 F4 1A 26 1B"); // Everything else, unknown

            data.WriteHexString("DB 1D F6 05 00 00 00 00 00 00 00 00"); // 2nd char data?*/

            int nochars = 0;

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM characters WHERE user = @userid;";
                    cmd.Parameters.AddWithValue("@userid", Methods.cleanString(uid));
                    nochars = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database error: " + ex);
                return;
            }

            data.WriteByte((byte)nochars); // No. chars

            try
            {
                byte currcard = 0;
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM characters WHERE user = @userid LIMIT 3;";
                    cmd.Parameters.AddWithValue("@userid", Methods.cleanString(uid));
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.WriteHexString("D8 1D F6 05 00 00 00 00 0B E1 F5 05 00 00 00 00");
                            data.WriteByte(currcard); // Card position (0 index)
                            data.WriteString(reader.GetString("name"), 16); // 16 byte character name
                            data.WriteHexString("00 00 00 00 01 00 01 00 04 02 01 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"); // ??
                            data.WriteByte((byte)reader.GetInt32("level")); // Level
                            data.WriteHexString("00 00 00 21 00 00 00 00 00 00 00 00 00 21 00");
                            data.WriteUInt32(reader.GetUInt32("money")); // Current galders
                            data.WriteUshort(reader.GetUInt16("health")); // HP
                            data.WriteUshort(reader.GetUInt16("mana")); // MP
                            data.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 F0 A0 E2 6A E4 73 D1 01 F0 A0 E2 6A E4 73 D1 01 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

                            currcard++;
                        }
                    }
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database error: " + ex);
                return;
            }

            data.WriteByte((byte)nochars); // Amount of characters again?

            for (int i = 0; i != nochars; i++)
            {
                if (i == 0)
                {
                    data.WriteHexString("D8 1D F6 05 00 00 00 00 60 00 00 00 F4 1A 26 1B"); // Everything else, unknown
                }
                else {
                    data.WriteHexString("DB 1D F6 05 00 00 00 00 00 00 00 00"); // 2nd char data?
                }
            }

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
