using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu.Packets
{
    class CharacterList
    {
        public static void Handle(Socket sock, byte[] packet)
        {
            byte[][] udetail = Methods.Split(0x00, packet).ToArray();

            if (udetail.Length < 3)
            {
                // Invalid packet data
                return;
            }

            var user = new User
            {
                Username = ASCIIEncoding.ASCII.GetString(udetail[0]),
                Version = ASCIIEncoding.ASCII.GetString(udetail[2]),
                Socket = sock
            };

            if (!Program._clientSocketIdentifiers.ContainsKey(sock.GetHashCode()))
            {
                Program._clientSocketIdentifiers.Add(sock.GetHashCode(), user);
            }

            Program.logger.Debug("User ID: {0}", user.Username);
            Program.logger.Debug("User PW: {0}", ASCIIEncoding.ASCII.GetString(udetail[1]));
            Program.logger.Debug("Client Version: {0}", user.Version);

            PacketBuffer data = new PacketBuffer(sock);
            data.WriteHeaderHexString("D2 07 00 00 01");

            int nochars = 0;

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM characters WHERE user = @userid;";
                    cmd.Parameters.AddWithValue("@userid", Methods.cleanString(user.Username));
                    nochars = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
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
                    cmd.Parameters.AddWithValue("@userid", Methods.cleanString(user.Username));
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.WriteUInt32(reader.GetUInt32("id"));
                            data.WriteHexString("00 00 00 00");
                            data.WriteUInt32(reader.GetUInt32("ownerid"));
                            data.WriteHexString("00 00 00 00");
                            data.WriteByte(currcard); // Card position (0 index)
                            data.WriteString(reader.GetString("name"), 16); // 16 byte character name
                            data.WriteHexString("00 00 00 00");
                            data.WriteByte((byte)reader.GetInt32("type"));
                            data.WriteByte(0x00);
                            data.WriteByte((byte)reader.GetInt32("job"));
                            data.WriteByte((byte)reader.GetInt32("ftype"));

                            byte[] points = new byte[] { 0x04, 0x02, 0x01, 0x03 };

                            int ptIdx = 0;
                            foreach (string pt in reader.GetString("build").Split(' '))
                            {
                                try
                                {
                                    points[ptIdx] = (byte)int.Parse(pt);
                                }
                                catch
                                { }

                                ptIdx++;
                            }

                            data.WriteByteArray(points);
                            data.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00"); // ??
                            data.WriteByte((byte)reader.GetInt32("level")); // Level
                            data.WriteHexString("00 00 00 21 00 00 00 00 00 00 00 00 00 21 00");
                            data.WriteUInt32(reader.GetUInt32("money")); // Current galders
                            data.WriteUshort(reader.GetUInt16("health")); // HP
                            data.WriteUshort(reader.GetUInt16("mana")); // MP
                            data.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 F0 A0 E2 6A E4 73 D1 01 F0 A0 E2 6A E4 73 D1 01 00 00 00 00");
                            data.WriteByte((byte)reader.GetInt32("hair")); // Hair color
                            data.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                            charids[currcard] = reader.GetUInt32("id");

                            currcard++;
                        }
                    }
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
                return;
            }

            data.WriteByte((byte)nochars); // # chars

            for (int i = 0; i != nochars; i++)
            {
                if (i == 0)
                {
                    data.WriteUInt32(charids[i]);
                    data.WriteHexString("00 00 00 00 60 00 00 00 F4 1A 26 1B"); // Everything else, unknown
                }
                else
                {
                    data.WriteUInt32(charids[i]);
                    data.WriteHexString("00 00 00 00 00 00 00 00"); // 2nd char data?
                }
            }

            byte[] msg = data.getPacket();
            sock.Send(msg);
        }
    }
}
