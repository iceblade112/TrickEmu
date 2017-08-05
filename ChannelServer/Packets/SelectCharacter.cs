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
    class SelectCharacter
    {
        public static void Handle(Socket sock, byte[] packet)
        {
            uint userId = BitConverter.ToUInt32(packet, 0);

            PacketBuffer msg1 = new PacketBuffer(sock);
            msg1.WriteHeaderHexString("DE 07 00 00 00");
            msg1.WriteUInt32(userId);
            msg1.WriteHexString("00 00 00 00 0A 00 3C 15 00 00");
            msg1.WriteString(Program.config.Server["GameIP"]);
            msg1.WriteHexString("00 F6 55 00 00 00 00 00 00 00 00 00 00 09 00 00 00 86 15 00 00 00");
            msg1.Send(false);

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT characters.id, users.id AS ownerid FROM characters INNER JOIN users ON characters.user = users.username WHERE characters.id = @charid;";
                cmd.Parameters.AddWithValue("@charid", userId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userId = reader.GetUInt32("ownerid");
                    }
                }

            }

            PacketBuffer msg2 = new PacketBuffer(sock);
            msg2.WriteHeaderHexString("03 00 00 00 00");
            msg2.WriteHexString("00 00");
            msg2.WriteUInt32(userId);
            msg2.WriteHexString("00 00 00 00");
            msg2.Send(false);

            if (Program.config.Server["SystemEnabled"] == "1" || Program.config.Server["SystemEnabled"].ToLower() == "true")
            {
                PacketBuffer sysserver = new PacketBuffer(sock);
                sysserver.WriteHeaderHexString("85 15 00 00 01");
                sysserver.WriteHexString("E0 1D F6 05 00 00 00 00");
                sysserver.WriteString(Program.config.Server["SystemIP"]);
                sysserver.WriteByte(0x00);
                sysserver.WriteUshort(ushort.Parse(Program.config.Server["SystemPort"]));
                sysserver.WriteHexString("00 00 6C 3D 00 00");
                sysserver.Send();
            }
        }
    }
}
