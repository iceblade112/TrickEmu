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

            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("D2 07 00 00 01");

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

            UInt32[] charids = new UInt32[16];

            try
            {
                byte currcard = 0;
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT characters.*, users.id AS ownerid FROM characters INNER JOIN users ON characters.user = users.username WHERE user = @userid LIMIT 3;";
                    cmd.Parameters.AddWithValue("@userid", Methods.cleanString(uid));
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.WriteUInt32(reader.GetUInt32("id"));
	                        data.WriteHexString("00 00 00 00");
	                        data.WriteUInt32(reader.GetUInt32("ownerid"));
	                        data.WriteHexString("00 00 00 00");
                            //data.WriteHexString("D8 1D F6 05 00 00 00 00 0B E1 F5 05 00 00 00 00");
                            data.WriteByte(currcard); // Card position (0 index)
                            data.WriteString(reader.GetString("name"), 16); // 16 byte character name
                            data.WriteHexString("00 00 00 00 01 00 01 00 04 02 01 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"); // ??
                            data.WriteByte((byte)reader.GetInt32("level")); // Level
                            data.WriteHexString("00 00 00 21 00 00 00 00 00 00 00 00 00 21 00");
                            data.WriteUInt32(reader.GetUInt32("money")); // Current galders
                            data.WriteUshort(reader.GetUInt16("health")); // HP
                            data.WriteUshort(reader.GetUInt16("mana")); // MP
                            data.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 F0 A0 E2 6A E4 73 D1 01 F0 A0 E2 6A E4 73 D1 01 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

                            charids[currcard] = reader.GetUInt32("id");

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
                    data.WriteUInt32(charids[i]);
                    data.WriteHexString("00 00 00 00 60 00 00 00 F4 1A 26 1B"); // Everything else, unknown
                    //data.WriteHexString("D8 1D F6 05 00 00 00 00 60 00 00 00 F4 1A 26 1B"); // Everything else, unknown
                }
                else
                {
                    data.WriteUInt32(charids[i]);
                    data.WriteHexString("00 00 00 00 00 00 00 00"); // 2nd char data?
                    //data.WriteHexString("DB 1D F6 05 00 00 00 00 00 00 00 00"); // 2nd char data?
                }
            }

            byte[] msg = data.getPacket();
            sock.Send(msg);
        }

        public static void SelectCharIngame(byte[] dec, Socket sock)
        {
            // Char select (to go ingame)
            // Send character ID, IP of GS, etc

            // Recv: [UInt32 ID] 00 00 00 00

            PacketBuffer msg1 = new PacketBuffer();
            msg1.WriteHeaderHexString("DE 07 00 00 00");
            msg1.WriteByte(dec[0]);
            msg1.WriteByte(dec[1]);
            msg1.WriteByte(dec[2]);
            msg1.WriteByte(dec[3]);
            msg1.WriteHexString("00 00 00 00 0A 00 3C 15 00 00 31 32 37 2E 30 2E 30 2E 31 00 F6 55 00 00 00 00 00 00 00 00 00 00 09 00 00 00 86 15 00 00 00");

            sock.Send(msg1.getPacketDecrypted());

	        UInt32 userId = 0;

	        using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
	        {
		        cmd.CommandText = "SELECT characters.id, users.id AS ownerid FROM characters INNER JOIN users ON characters.user = users.username WHERE characters.id = @charid;";
		        cmd.Parameters.AddWithValue("@charid", BitConverter.ToUInt32(new byte[] { dec[0], dec[1], dec[2], dec[3] }, 0));
		        using (MySqlDataReader reader = cmd.ExecuteReader())
		        {
			        while (reader.Read())
			        {
				        userId = reader.GetUInt32("ownerid");
			        }
		        }

	        }

	        PacketBuffer msg2 = new PacketBuffer();
	        msg2.WriteHeaderHexString("03 00 00 00 00");
	        msg2.WriteHexString("00 00");
	        msg2.WriteUInt32(userId);
	        msg2.WriteHexString("00 00 00 00");

	        //byte[] msg2 = new byte[] { 0x13, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0xE1, 0xF5, 0x05, 0x00, 0x00, 0x00, 0x00 };
            sock.Send(msg2.getPacketDecrypted());

            PacketBuffer sysserver = new PacketBuffer();
            sysserver.WriteHeaderHexString("85 15 00 00 01");
            sysserver.WriteHexString("E0 1D F6 05 00 00 00 00 31 32 37 2E 30 2E 30 2E 31 00 18 34 00 00 6C 3D 00 00");
            sock.Send(sysserver.getPacket());
        }
    }
}
