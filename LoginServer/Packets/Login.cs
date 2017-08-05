using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TELib;

namespace TrickEmu.Packets
{
    class Login
    {
        public static void Handle(Socket sock, byte[] packet)
        {
            string uid = Methods.getString(packet, packet.Length).Substring(0, 12);
            string upw = Methods.getString(packet, packet.Length).Substring(19);

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = @userid AND password = @userpw;";
                    cmd.Parameters.AddWithValue("@userid", Methods.cleanString(uid));
                    cmd.Parameters.AddWithValue("@userpw", Methods.cleanString(upw));
                    if (Convert.ToInt32(cmd.ExecuteScalar()) >= 1)
                    {
                        // Send server select
                        PacketBuffer data = new PacketBuffer(sock);
                        data.WriteHeaderHexString("EE 2C 00 00 00 0B");
                        //data.WriteHexString("00 00 00 00 00 00 00 00 00 00 00");

                        data.WriteBytePad(0x00, 12);
                        data.WriteByte(1); // Amount of channels
                        data.WriteByte(1); // Amount of worlds

                        data.WriteByte(0x01); // World 1
                        data.WriteByte(0x00); // Padding

                        data.WriteString("Earth", 32);
                        data.WriteByte(0x00);
                        data.WriteString("PRC China", 32);

                        data.WriteByte(0x00);
                        data.WriteHexString("AC 0D");
                        data.WriteBytePad(0x00, 4);

                        data.Send(false);
                    }
                    else
                    {
                        sock.Send(new byte[] { 0x0D, 0x00, 0x00, 0x00, 0xEF, 0x2C, 0x00, 0x00, 0x00, 0x63, 0xEA, 0x00, 0x00 });
                    }
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
            }
        }
    }
}
