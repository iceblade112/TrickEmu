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
    class DeleteCharacter
    {
        public static void Handle(Socket sock, byte[] packet)
        {
            uint userId = BitConverter.ToUInt32(packet, 0);

            Program.logger.Debug("Deleting " + userId + ".");

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM characters WHERE id = @id;";
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteScalar();
                }

                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM char_equip WHERE id = @id;";
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteScalar();
                }

                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM item_common WHERE owner = @id;";
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteScalar();
                }

                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM item_rare WHERE owner = @id;";
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteScalar();
                }

                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM item_drill WHERE owner = @id;";
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteScalar();
                }

                PacketBuffer delpkt = new PacketBuffer(sock);
                delpkt.WriteHeaderHexString("DB 07 00 00 01");
                delpkt.Send();
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
                return;
            }
        }
    }
}
