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
    class CharacterSelect
    {
        public static void SelectChar1(Socket sock, byte[] packet)
        {
            uint charId = BitConverter.ToUInt32(packet, 0);

            Program._entityIdx++;

            Character plr = new Character
            {
                Socket = sock,
                ID = charId,
                EntityID = (ushort)Program._entityIdx
            };

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM characters WHERE id = @userid LIMIT 1;";
                cmd.Parameters.AddWithValue("@userid", plr.ID);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        plr.Username = reader.GetString("user");
                        plr.Name = reader.GetString("name");
                        plr.Level = reader.GetInt32("level");
                        plr.Money = reader.GetUInt32("money");
                        plr.HP = reader.GetInt16("health");
                        plr.MP = reader.GetInt16("mana");
                        plr.Map = reader.GetInt16("map");
                        plr.PosX = reader.GetUInt16("pos_x");
                        plr.PosY = reader.GetUInt16("pos_y");
                        plr.Job = reader.GetInt32("job");
                        plr.Type = reader.GetInt32("type");
                        plr.FType = reader.GetInt32("ftype");
                        plr.Build = reader.GetString("build");
                        plr.Hair = reader.GetInt32("hair");
                    }
                }
                cmd.Dispose();
            }
            Program.logger.Debug(plr.Name + " is entering the world.");

            Program._clientPlayers.Add(sock.GetHashCode(), plr);

            List<byte> msg = new List<byte>();
            // E6 00 00 00 01
            // Head
            PacketBuffer head = new PacketBuffer();
            head.WriteHeaderHexString("E6 00 00 00 01");
            head.WriteHexString("9E 82 07 01");
            head.WriteUshort(plr.EntityID);
            // PosX, PosY
            head.WriteUshort(plr.PosX);
            head.WriteUshort(plr.PosY);

            // E7 00 00 00 01
            // Item
            PacketBuffer item = new PacketBuffer();
            item.WriteHeaderHexString("E7 00 00 00 01");

            PacketBuffer itemPartial = new PacketBuffer();
            ushort itemCount = 0;
            
            //Program.logger.Debug("Item packet: " + BitConverter.ToString(item.getPacketDecrypted()).Replace("-", " "));

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM item_common WHERE owner = @userId;";
                cmd.Parameters.AddWithValue("@userId", plr.ID);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itemPartial.WriteByte(0x01); // Item DB type (Common)
                        itemPartial.WriteUshort(reader.GetUInt16("item_id"));
                        itemPartial.WriteUInt32(reader.GetUInt32("id"));
                        itemPartial.WriteHexString("00 00 00 00");
                        itemPartial.WriteUshort(reader.GetUInt16("item_count"));

                        itemCount++;
                    }
                }
            }

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM item_rare WHERE owner = @userId;";
                cmd.Parameters.AddWithValue("@userId", plr.ID);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itemPartial.WriteByte(0x02); // Item DB type (Rare)
                        itemPartial.WriteUshort(reader.GetUInt16("item_id"));
                        itemPartial.WriteUInt32(reader.GetUInt32("id"));
                        itemPartial.WriteHexString("00 00 00 00");
                        itemPartial.WriteUshort(1, false);

                        itemCount++;
                    }
                }
            }

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM item_drill WHERE owner = @userId;";
                cmd.Parameters.AddWithValue("@userId", plr.ID);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itemPartial.WriteByte(0x03); // Item DB type (Drill)
                        itemPartial.WriteUshort(reader.GetUInt16("item_id")); // Item ID - 40 1F
                        itemPartial.WriteUInt32(reader.GetUInt32("id")); // Item UID FLIPPED - 2000000180 (B4 94 35 77)
                        itemPartial.WriteHexString("00 00 00 00");
                        itemPartial.WriteUshort(1, false); // Item count: 01 00
                        itemPartial.WriteByte(0x00);
                        itemPartial.WriteUshort(reader.GetUInt16("item_life"));

                        itemCount++;
                    }
                }
            }

            item.WriteUshort(itemCount);
            item.WriteByteArray(itemPartial.getPacketDecrypted(false));

            //Program.logger.Debug($"itemPartial: {BitConverter.ToString(itemPartial.getPacketDecrypted(false)).Replace("-", " ")}.");
            //Program.logger.Debug($"item: {BitConverter.ToString(item.getPacketDecrypted()).Replace("-", " ")}.");

            // TODO: move to own class
            uint activeEar = 0;
            uint activeTail = 0;

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM char_equip WHERE id = @userId;";
                cmd.Parameters.AddWithValue("@userId", plr.ID);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            activeEar = reader.GetUInt32("ears");
                        }
                        catch (Exception ex)
                        {
                            Program.logger.Error(ex, $"Could not get active ears for UID {plr.ID}.");
                        }

                        try
                        {
                            activeTail = reader.GetUInt32("tail");
                        }
                        catch (Exception ex)
                        {
                            Program.logger.Error(ex, $"Could not get active tail for UID {plr.ID}.");
                        }
                    }
                }
            }

            // E9 00 00 00 01
            // Stat
            PacketBuffer stat = new PacketBuffer();
            stat.WriteHeaderHexString("E9 00 00 00 01");
            stat.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            stat.WriteUInt32(activeEar); // Ear UID - 6900
            stat.WriteHexString("00 00 00 00");
            stat.WriteUInt32(activeTail); // Tail UID - 6950
            stat.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00");
            stat.WriteByte((byte)plr.Map); // Map
            stat.WriteHexString("00 D8 04 00 00 E8 06 00 00 21 00 00 00 00 00 78 00 50 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 30 53 C9 CC E4 65 D1 01 30 53 C9 CC E4 65 D1 01 34 06 00 00");
            stat.WriteByte((byte)plr.Hair);

            Program.logger.Debug($"stat: {BitConverter.ToString(stat.getPacketDecrypted()).Replace("-", " ")}.");

            // EA 00 00 00 01
            // Quest
            PacketBuffer quest = new PacketBuffer();
            quest.WriteHeaderHexString("EA 00 00 00 01");
            quest.WriteHexString("00 00 00 00"); // No quests

            // EB 00 00 00 01
            // MonsterQuest
            PacketBuffer monquest = new PacketBuffer();
            monquest.WriteHeaderHexString("EB 00 00 00 01");
            monquest.WriteHexString("00 00 00 00");

            // 28 01 00 00 01
            // SkillQuest
            PacketBuffer skillquest = new PacketBuffer();
            skillquest.WriteHeaderHexString("28 01 00 00 01");
            skillquest.WriteHexString("00 00");

            // 29 01 00 00 01
            // SkillMonsterQuest
            PacketBuffer skillmonquest = new PacketBuffer();
            skillmonquest.WriteHeaderHexString("29 01 00 00 01");
            skillmonquest.WriteHexString("00 00");

            // 0F 01 00 00 01
            // Skill
            PacketBuffer skill = new PacketBuffer();
            skill.WriteHeaderHexString("0F 01 00 00 01");
            skill.WriteHexString("00 00");

            // 10 01 00 00 01
            // SkillSlot
            PacketBuffer skillslot = new PacketBuffer();
            skillslot.WriteHeaderHexString("10 01 00 00 01");
            skillslot.WriteHexString("00 00 00 00 00 00");

            msg.AddRange(head.getPacket());
            msg.AddRange(item.getPacket());
            msg.AddRange(stat.getPacket());
            msg.AddRange(quest.getPacket());
            msg.AddRange(monquest.getPacket());
            msg.AddRange(skillquest.getPacket());
            msg.AddRange(skillmonquest.getPacket());
            msg.AddRange(skill.getPacket());
            msg.AddRange(skillslot.getPacket());
            msg.AddRange(new byte[] { 0x09, 0x00, 0x02, 0x35, 0x05 }); // Unknown
            msg.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00 });

            sock.Send(msg.ToArray());
        }

        public static void SelectChar2(Socket sock, byte[] packet)
        {
            var thisChar = Program._clientPlayers[sock.GetHashCode()];
            Program.logger.Debug("Entity ID {0} is entering", thisChar.EntityID);

            List<byte> msg1 = new List<byte>();

            PacketBuffer msg01 = new PacketBuffer();
            msg01.WriteHeaderHexString("07 00 00 00 01");
            msg01.WriteUshort(thisChar.EntityID);
            msg01.WriteByte((byte)thisChar.Type);
            msg01.WriteByte(0x00);
            msg01.WriteByte((byte)thisChar.Job);
            msg01.WriteByte((byte)thisChar.FType);
            msg01.WriteByte((byte)thisChar.Hair);
            msg01.WriteHexString("40 00");
            msg01.WriteUshort(thisChar.PosX);
            msg01.WriteUshort(thisChar.PosY);
            msg01.WriteUshort(thisChar.PosX);
            msg01.WriteUshort(thisChar.PosY);
            msg01.WriteString(thisChar.Name);
            msg01.WriteHexString("00 00 80 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 00");
            msg1.AddRange(msg01.getPacket());
            msg1.AddRange(new byte[] { 0x09, 0x00, 0xE2, 0x88, 0x35, 0x01, 0x00, 0x00, 0x00 });

            sock.Send(msg1.ToArray());

            // Get every player connected to the server
            // Send existing players to new client
            foreach (KeyValuePair<int, Character> entry in Program._clientPlayers)
            {
                if (entry.Value.Map != thisChar.Map) continue;

                // If the player selected isn't the current player
                if (entry.Key != sock.GetHashCode())
                {
                    Program.logger.Debug("Sending entity ID {0} to {1}", thisChar.EntityID, entry.Value.EntityID);
                    PacketBuffer msg2z = new PacketBuffer();
                    msg2z.WriteHeaderHexString("07 00 00 00 01");
                    msg2z.WriteUshort(entry.Value.EntityID);
                    msg2z.WriteByte((byte)entry.Value.Type);
                    msg2z.WriteByte(0x00);
                    msg2z.WriteByte((byte)entry.Value.Job);
                    msg2z.WriteByte((byte)entry.Value.FType);
                    msg2z.WriteByte((byte)entry.Value.Hair);
                    msg2z.WriteHexString("40 00");
                    msg2z.WriteUshort(entry.Value.PosX);
                    msg2z.WriteUshort(entry.Value.PosY);
                    msg2z.WriteUshort(entry.Value.PosX);
                    msg2z.WriteUshort(entry.Value.PosY);
                    msg2z.WriteString(entry.Value.Name);
                    msg2z.WriteHexString("00 00 80 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 00");

                    sock.Send(msg2z.getPacket());
                }
            }

            // At this point, existing players don't know about this new player.
            // Get every player connected to the server
            // Send new player to existing
            foreach (KeyValuePair<int, Character> entry in Program._clientPlayers)
            {
                if (entry.Value.ClientRemoved)
                    continue;

                if (entry.Value.Map != thisChar.Map) continue;

                try
                {
                    // If the player selected isn't the current player
                    if (entry.Key != sock.GetHashCode())
                    {
                        Program.logger.Debug("Sending entity ID {0} to {1}", entry.Value.EntityID, thisChar.EntityID);
                        PacketBuffer msg3z = new PacketBuffer();
                        msg3z.WriteHeaderHexString("07 00 00 00 01");
                        msg3z.WriteUshort(thisChar.EntityID);
                        msg3z.WriteByte((byte)thisChar.Type);
                        msg3z.WriteByte(0x00);
                        msg3z.WriteByte((byte)thisChar.Job);
                        msg3z.WriteByte((byte)thisChar.FType);
                        msg3z.WriteByte((byte)thisChar.Hair);
                        msg3z.WriteHexString("40 00");
                        msg3z.WriteUshort(thisChar.PosX);
                        msg3z.WriteUshort(thisChar.PosY);
                        msg3z.WriteUshort(thisChar.PosX);
                        msg3z.WriteUshort(thisChar.PosY);
                        msg3z.WriteString(thisChar.Name);
                        msg3z.WriteHexString("00 00 80 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 00");

                        entry.Value.Socket.Send(msg3z.getPacket());
                    }
                }
                catch (ObjectDisposedException)
                {
                    continue;
                }
            }

            var fullMsg2 = new List<byte>();

            PacketBuffer msg2 = new PacketBuffer();
            msg2.WriteHeaderHexString("24 00 00 00 01");
            msg2.WriteHexString("78 00");

            PacketBuffer msg21 = new PacketBuffer();
            msg21.WriteHeaderHexString("25 00 00 00 01");
            msg21.WriteHexString("50 00");
            
            PacketBuffer msg22 = new PacketBuffer();
            msg22.WriteHeaderHexString("25 00 00 00 01");
            msg22.WriteHexString("50 00");

            PacketBuffer msg23 = new PacketBuffer();
            msg23.WriteHeaderHexString("CC 01 00 00 01");
            msg23.WriteHexString("43 00 01 03 00 01");

            PacketBuffer msg24 = new PacketBuffer();
            msg24.WriteHeaderHexString("CD 01 00 00 01");
            msg24.WriteHexString("43 00 BF C9 D2 D4 BF B4 B5 BD B5 D8 CF C2 A1 A3 00");

            fullMsg2.AddRange(msg2.getPacket());
            fullMsg2.AddRange(msg21.getPacket());
            fullMsg2.AddRange(msg22.getPacket());
            fullMsg2.AddRange(msg23.getPacket());
            fullMsg2.AddRange(msg24.getPacket());
            sock.Send(fullMsg2.ToArray());

            PacketBuffer msg3 = new PacketBuffer();
            msg3.WriteHeaderHexString("42 00 00 00 01");
            msg3.WriteHexString("40 00 A0");
            sock.Send(msg3.getPacket());
            
            GameFunctions.SendChat("This is the MOTD.", sock);
        }
    }
}
