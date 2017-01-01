using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class CreateCharacter
    {
        public static void CreateItem(uint charId, int itemId, int itemCount, int itemType, bool wearing = false, string wearType = "")
        {
            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    if(itemType == 1)
                    {
                        // Common
                        cmd.CommandText = "INSERT INTO item_common (owner, item_id, item_count) VALUES (@charId, @itemId, @itemCt); select last_insert_id();";
                        cmd.Parameters.AddWithValue("@charId", charId);
                        cmd.Parameters.AddWithValue("@itemId", itemId);
                        cmd.Parameters.AddWithValue("@itemCt", itemCount);
                    }
                    else if(itemType == 2)
                    {
                        // Rare
                        cmd.CommandText = "INSERT INTO item_rare (owner, item_id, wearing) VALUES (@charId, @itemId, @wearing); select last_insert_id();";
                        cmd.Parameters.AddWithValue("@charId", charId);
                        cmd.Parameters.AddWithValue("@itemId", itemId);
                        cmd.Parameters.AddWithValue("@wearing", (wearing ? 1 : 0));
                    }
                    else if (itemType == 3)
                    {
                        // Rare
                        cmd.CommandText = "INSERT INTO item_drill (owner, item_id, item_life) VALUES (@charId, @itemId, @itemLife); select last_insert_id();";
                        cmd.Parameters.AddWithValue("@charId", charId);
                        cmd.Parameters.AddWithValue("@itemId", itemId);
                        cmd.Parameters.AddWithValue("@itemLife", itemCount);
                    }

                    uint newItemId = Convert.ToUInt32(cmd.ExecuteScalar());

                    if (itemType == 2 && wearType != "")
                    {
                        Program.logger.Debug($"Setting item {newItemId} active (type {wearType}) for character {charId}.");
                        MySqlCommand cmd2 = Program._MySQLConn.CreateCommand();
                        cmd2.CommandText = "UPDATE char_equip SET " + wearType + " = @itemId WHERE id = @charId;";
                        cmd2.Parameters.AddWithValue("@charId", charId);
                        cmd2.Parameters.AddWithValue("@itemId", newItemId);
                        cmd2.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
                return;
            }
        }

        public static void Handle(Socket sock, byte[] packet)
        {
            if (!Program._clientSocketIdentifiers.ContainsKey(sock.GetHashCode()))
            {
                Program.logger.Warn("{0} does not exist in the identifier dictionary!", sock.GetHashCode());
                return;
            }

            string uid = Program._clientSocketIdentifiers[sock.GetHashCode()].Username;
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
                        Program.logger.Debug("This guy already has 3+ characters!");
                        cmd.Dispose();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
                return;
            }

            byte[][] data = Methods.Split(0x00, packet).ToArray();

            string charName = Encoding.GetEncoding("gb2312").GetString(data[0]);
            // Investigate: won't work with a non-bunny character (sheep)
            // \/

            var charData = packet.Skip(charName.Length + 1).ToArray();

            /*
             *
             * 0: charType
             * 2: charHairColor
             * 3: charJob
             * 4: charFType
             * 
             */

            int charJob = charData[3];
            int charType = charData[0];
            int charFType = charData[4];

            int charHair = charData[2];

            byte[] charPoints = { charData[5], charData[6], charData[7], charData[8] };

            string charBuild = "";

            foreach(byte pt in charPoints)
            {
                charBuild += ((int)pt).ToString() + ",";
            }

            charBuild = charBuild.Substring(0, charBuild.Length - 1);

            uint newCharId = 0;

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO characters (user, name, job, type, ftype, hair, build) VALUES (@userid, @charname, @job, @type, @ftype, @hair, @build); select last_insert_id();";
                    cmd.Parameters.AddWithValue("@userid", uid);
                    cmd.Parameters.AddWithValue("@charname", charName);
                    cmd.Parameters.AddWithValue("@job", charJob);
                    cmd.Parameters.AddWithValue("@type", charType);
                    cmd.Parameters.AddWithValue("@ftype", charFType);
                    cmd.Parameters.AddWithValue("@hair", charHair);
                    cmd.Parameters.AddWithValue("@build", charBuild);
                    newCharId = Convert.ToUInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
                return;
            }

            // Create equip column

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO char_equip (id) VALUES (@charId);";
                    cmd.Parameters.AddWithValue("@charId", newCharId);
                    cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
                return;
            }

            // Create default items

            CreateItem(newCharId, 2006, 50, 1);
            CreateItem(newCharId, 2206, 50, 1);

            switch(charType)
            {
                default: break;
                case 1:
                    // Bunny
                    CreateItem(newCharId, 6900, 1, 2, true, "ears");
                    CreateItem(newCharId, 6950, 1, 2, true, "tail");
                    break;
                case 2:
                    // Buffalo
                    CreateItem(newCharId, 6901, 1, 2, true, "ears");
                    CreateItem(newCharId, 6951, 1, 2, true, "tail");
                    break;
                case 3:
                    // Sheep
                    CreateItem(newCharId, 6902, 1, 2, true, "ears");
                    CreateItem(newCharId, 6952, 1, 2, true, "tail");
                    break;
                case 4:
                    // Dragon
                    CreateItem(newCharId, 6903, 1, 2, true, "ears");
                    CreateItem(newCharId, 6953, 1, 2, true, "tail");
                    break;
                case 5:
                    // Fox
                    CreateItem(newCharId, 6904, 1, 2, true, "ears");
                    CreateItem(newCharId, 6954, 1, 2, true, "tail");
                    break;
                case 6:
                    // Lion
                    CreateItem(newCharId, 6905, 1, 2, true, "ears");
                    CreateItem(newCharId, 6955, 1, 2, true, "tail");
                    break;
                case 7:
                    // Cat
                    CreateItem(newCharId, 6906, 1, 2, true, "ears");
                    CreateItem(newCharId, 6956, 1, 2, true, "tail");
                    break;
                case 8:
                    // Raccoon
                    CreateItem(newCharId, 6907, 1, 2, true, "ears");
                    CreateItem(newCharId, 6907, 1, 2, true, "tail");
                    break;
            }

            CreateItem(newCharId, 8000, 600, 3);

            // Get new char card position
            byte currCard = 0;
            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT characters.*, users.id AS ownerid FROM characters INNER JOIN users ON characters.user = users.username WHERE user = @userid LIMIT 3;";
                cmd.Parameters.AddWithValue("@userid", Methods.cleanString(Program._clientSocketIdentifiers[sock.GetHashCode()].Username));
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        currCard++;
                    }
                }
            }

            PacketBuffer pdata = new PacketBuffer(sock);
            pdata.WriteHeaderHexString("D8 07 00 00 01");
            pdata.WriteUInt32(newCharId); // 4 bytes: character ID, flipped
            pdata.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00");
            pdata.WriteByte((byte)(currCard - 1)); // 0x02, the card index? wtf?
            pdata.WriteString(charName, 16);
            pdata.WriteHexString("00 00 00 00");
            pdata.WriteByte((byte)charType); // 0x07
            pdata.WriteByte(0x00);
            pdata.WriteByte((byte)charJob); // 0x07
            pdata.WriteByte((byte)charFType); // 0x03
            pdata.WriteByteArray(charPoints);
            pdata.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 21 00 00 00 00 00 00 00 00 00 21 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            pdata.WriteByte((byte)charHair);
            pdata.WriteBytePad(0x00, 152);
            
            pdata.Send();
        }
    }
}
