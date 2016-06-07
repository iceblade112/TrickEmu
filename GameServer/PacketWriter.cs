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
        public static void SelectChar1(byte[] dec, Socket sock)
        {
            Program._entityIdx++;

            Player plr = new Player();
            plr.ClientSocket = sock;
            plr.ID = BitConverter.ToUInt32(new byte[] { dec[0], dec[1], dec[2], dec[3] }, 0); // ... soon
            plr.EntityID = (ushort)Program._entityIdx;

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT name, level, money, health, mana, map, pos_x, pos_y FROM characters WHERE id = @userid LIMIT 1;";
                cmd.Parameters.AddWithValue("@userid", plr.ID);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        plr.Name = reader.GetString("name");
                        plr.Level = reader.GetInt32("level");
                        plr.Money = reader.GetUInt32("money");
                        plr.HP = reader.GetInt16("health");
                        plr.MP = reader.GetInt16("mana");
                        plr.Map = reader.GetInt16("map");
                        plr.PosX = reader.GetUInt16("pos_x");
                        plr.PosY = reader.GetUInt16("pos_y");
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
            head.WriteHexString("9E 82 07 01"); // 95 1B"); // D8 04 E8 06");
            head.WriteUshort(plr.EntityID);
            // PosX, PosY
            head.WriteUshort(plr.PosX);
            head.WriteUshort(plr.PosY);

            // E7 00 00 00 01
            // Item
            PacketBuffer item = new PacketBuffer();
            item.WriteHeaderHexString("E7 00 00 00 01");
            item.WriteHexString("05 00 01 D6 07 EF D1 8C 77 00 00 00 00 32 00 01 9E 08 F0 D1 8C 77 00 00 00 00 32 00 02 F4 1A ED D1 8C 77 00 00 00 00 01 00 02 26 1B EE D1 8C 77 00 00 00 00 01 00 03 40 1F EC D1 8C 77 00 00 00 00 01 00 58 02");

            // E9 00 00 00 01
            // Stat
            PacketBuffer stat = new PacketBuffer();
            stat.WriteHeaderHexString("E9 00 00 00 01");
            //stat.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ED D1 8C 77 00 00 00 00 EE D1 8C 77 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 21 00 D8 04 00 00 E8 06 00 00 21 00 00 00 00 00 78 00 50 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 30 53 C9 CC E4 65 D1 01 30 53 C9 CC E4 65 D1 01 34 06 00 00 01");
            stat.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ED D1 8C 77 00 00 00 00 EE D1 8C 77 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00");
            stat.WriteByte((byte)plr.Map); // Map
            stat.WriteHexString("00 D8 04 00 00 E8 06 00 00 21 00 00 00 00 00 78 00 50 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 30 53 C9 CC E4 65 D1 01 30 53 C9 CC E4 65 D1 01 34 06 00 00 01");

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

        public static void SelectChar2(byte[] dec, Socket sock)
        {
            Program.logger.Debug("Entity ID {0} is entering", Program._clientPlayers[sock.GetHashCode()].EntityID);

            List<byte> msg1 = new List<byte>();

            PacketBuffer msg01 = new PacketBuffer();
            msg01.WriteHeaderHexString("07 00 00 00 01");
            msg01.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            msg01.WriteHexString("01 00 01 00 00 40 00");
            msg01.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
            msg01.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
            msg01.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
            msg01.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
            msg01.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
            msg01.WriteHexString("00 00 80 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 00");
            msg1.AddRange(msg01.getPacket());
            msg1.AddRange(new byte[] { 0x09, 0x00, 0xE2, 0x88, 0x35, 0x01, 0x00, 0x00, 0x00 });

            sock.Send(msg1.ToArray());

            // Get every player connected to the server
            // Send existing players to new client
            foreach (KeyValuePair<int, Player> entry in Program._clientPlayers)
            {
                if (entry.Value.Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                // If the player selected isn't the current player
                if (entry.Key != sock.GetHashCode())
                {
                    Program.logger.Debug("Sending entity ID {0} to {1}", Program._clientPlayers[sock.GetHashCode()].EntityID, entry.Value.EntityID);
                    PacketBuffer msg2z = new PacketBuffer();
                    msg2z.WriteHeaderHexString("07 00 00 00 01");
                    //msg1.WriteHexString("95 1B");
                    msg2z.WriteUshort(entry.Value.EntityID);
                    msg2z.WriteHexString("01 00 01 00 00 40 00");
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
            foreach (KeyValuePair<int, Player> entry in Program._clientPlayers)
            {
				if (entry.Value.ClientRemoved)
					continue;

                if (entry.Value.Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;
				
                try {
                    // If the player selected isn't the current player
                    if (entry.Key != sock.GetHashCode())
                    {
                        Program.logger.Debug("Sending entity ID {0} to {1}", entry.Value.EntityID, Program._clientPlayers[sock.GetHashCode()].EntityID);
                        PacketBuffer msg3z = new PacketBuffer();
                        msg3z.WriteHeaderHexString("07 00 00 00 01");
                        msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                        msg3z.WriteHexString("01 00 01 00 00 40 00");
                        msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
                        msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
                        msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
                        msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
                        msg3z.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
                        msg3z.WriteHexString("00 00 80 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 00");

                        entry.Value.ClientSocket.Send(msg3z.getPacket());
                    }
                }
                catch (ObjectDisposedException)
                {
                    //Program._clientPlayers.Remove(entry.Key);
                    continue;
                }
            }

            /*PacketBuffer msg1 = new PacketBuffer();
            msg1.WriteHeaderHexString("07 00 00 00 01");
            msg1.WriteHexString("94 1B 01 00 01 00 01 40 00");
            msg1.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
            msg1.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
            msg1.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
            msg1.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
            msg1.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
            msg1.WriteHexString("00 00 00 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 01 C8 EC 21 7F 4D 09 EC EC EC");
            sock.Send(msg1.getPacket());*/

            // 2?
            //byte[] msg2 = new byte[] { 0x0B, 0x00, 0x52, 0x36, 0x24, 0x00, 0x00, 0x00, 0x01, 0x80, 0x03, 0x0B, 0x00, 0x41, 0x47, 0x25, 0x00, 0x00, 0x00, 0x01, 0xBF, 0xCD, 0x2C, 0x00, 0x9E, 0xAB, 0x08, 0x00, 0x00, 0x00, 0x01, 0x4A, 0xA0, 0xD8, 0xA0, 0xDC, 0x01, 0xA0, 0xA0, 0xA0, 0xC6, 0x6A, 0x3F, 0x3C, 0xC6, 0x6A, 0x3F, 0x3C, 0x23, 0x26, 0x54, 0xF5, 0x6C, 0x99, 0x84, 0xCE, 0x54, 0x36, 0xFF, 0x55, 0xA0, 0xA0, 0x05, 0xA0, 0x6A, 0xA0, 0x2E, 0x00, 0xF5, 0x22, 0x08, 0x00, 0x00, 0x00, 0x01, 0x28, 0x0F, 0x1F, 0x0F, 0xD6, 0x91, 0x0F, 0x0F, 0x0F, 0xED, 0xAB, 0x46, 0x42, 0xED, 0xAB, 0x46, 0x42, 0xA3, 0x9A, 0x94, 0xD3, 0x2D, 0x76, 0xA3, 0xF1, 0x72, 0x68, 0xD8, 0xEE, 0x7C, 0xB6, 0x0F, 0x0F, 0xBA, 0x0F, 0x64, 0x0F };
            //sock.Send(msg2);

            PacketBuffer msg2 = new PacketBuffer();
            msg2.WriteHeaderHexString("24 00 00 00 01");
            msg2.WriteHexString("78 00 9A 3F 1C 51 48 3F 3F 3F D5 F3 08 63 3F 20 F9 83 3F 3F 3F D5 BD 18 FA 18 ED 7C 18 18 18 83 7C B4 1D 83 7C B4 1D 50 E0 88 45 F8 32 03 AE 7D 68 E9 50 B2 4A 18 18 DB 18 1D 18");
            sock.Send(msg2.getPacket());

            // 3...?
            //byte[] msg3 = new byte[] { 0x0C, 0x00, 0x7C, 0x33, 0x42, 0x00, 0x00, 0x00, 0x01, 0x6C, 0xD0, 0x8E };
            //sock.Send(msg3);

            PacketBuffer msg3 = new PacketBuffer();
            msg3.WriteHeaderHexString("42 00 00 00 01");
            msg3.WriteHexString("40 00 A0");
            sock.Send(msg3.getPacket());

            if (Program._MOTD.Length > 0)
            {
                foreach (string motd in Program._MOTD)
                {
                    ProjectMethods.SendChat(motd, sock);
                }
            }
        }

        public static void HeadNotice(byte[] dec, Socket sock)
        {
            // Head notice (personal notice)
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("B6 00 00 00 01"); // Packet header
            //data.WriteByte(0x95); // ???
            //data.WriteByte(0x1B); // ^
            data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            data.WriteString(Methods.sep(Methods.getString(dec, 9), "\x00"));
            data.WriteByte(0x00);

            Program.logger.Debug("Head notice text: {0}", Methods.sep(Methods.getString(dec, 9), "\x00"));

            sock.Send(data.getPacket());

            foreach (Socket sockt in Program._clientSockets)
            {
                // If not on the same map, don't broadcast
                if (Program._clientPlayers[sockt.GetHashCode()].Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                try
                {
                    sockt.Send(data.getPacket());
                }
                catch { }
            }
        }

        public static void Chat(byte[] dec, Socket sock)
        {
            // Chat
            // Packet data starts at 9
            // substring 9 (10)

            string chatString = Methods.sep(Methods.getString(dec, 0), "\x00");

            // Why am I OOP-ing this?
            if (Commands.Handler.Handle(chatString, sock)) return;

            if (chatString.StartsWith("!gmc "))
            {
                // GM chat

                PacketBuffer data = new PacketBuffer();
                data.WriteHeaderUshort(0x13C); // Packet ID
                data.WriteHeaderByteArray(new byte[] { 0x00, 0x00, 0x01 }); // Packet header padding
                data.WriteByteArray(new byte[] { 0x00, 0x00 }); // ?
                data.WriteString(Methods.sep(Methods.getString(dec, 0), "\x00").Substring(5)); // Actual chat message
                data.WriteByteArray(new byte[] { 0x00, 0x00 }); // ????

                byte[] newpkt = data.getPacket();

                foreach (Socket sockt in Program._clientSockets)
                {
                    try
                    {
                        if (sockt != sock)
                        {
                            sockt.Send(newpkt);
                        }
                    }
                    catch { }
                }

                sock.Send(newpkt);
            }
            else if (chatString.StartsWith("!oxc "))
            {
                // Normal chat
                PacketBuffer data = new PacketBuffer();
                data.WriteHeaderByteArray(new byte[] { 0xB5, 0x00, 0x00, 0x00, 0x01 });
                //data.WriteByteArray(new byte[] { 0x95, 0x1B }); //, 0x42, 0x6F, 0x62, 0x00 });
                data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                data.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
                data.WriteByte(0x00);
                data.WriteString(Methods.sep(Methods.getString(dec, dec.Length), "\x00").Substring(5));
                data.WriteByte(0x00);

                byte[] newpkt = data.getPacket();

                Program.logger.Debug("Sending to {0} client(s).", Program._clientSockets.ToArray().Length);

                foreach (Socket sockt in Program._clientSockets)
                {
                    try
                    {
                        if (sockt != sock)
                        {
                            sockt.Send(newpkt);
                        }
                    }
                    catch { }
                }

                sock.Send(newpkt);
            }
            else
            {
                // Normal chat
                PacketBuffer data = new PacketBuffer();
                data.WriteHeaderByteArray(new byte[] { 0x39, 0x00, 0x00, 0x00, 0x01 }); // Header (0x39 ID)
                //data.WriteByteArray(new byte[] { 0x94, 0x1B, 0x42, 0x6F, 0x62, 0x00 });
                //data.WriteByteArray(new byte[] { 0x95, 0x1B });
                data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                data.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
                data.WriteByte(0x00);
                data.WriteString(Methods.sep(Methods.getString(dec, dec.Length), "\x00"));
                data.WriteByte(0x00);

                byte[] newpkt = data.getPacket();

                foreach (Socket sockt in Program._clientSockets)
                {
                    try
                    {
                        // If not on the same map, don't broadcast
                        if (Program._clientPlayers[sockt.GetHashCode()].Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                        if (sockt != sock)
                        {
                            sockt.Send(newpkt);
                        }
                    }
                    catch { }
                }

                sock.Send(newpkt);
            }

            Program.logger.Debug("Received chat packet from entity {0}: {1}", Program._clientPlayers[sock.GetHashCode()].EntityID, chatString);
        }

        public static void MovePos(byte[] dec, Socket sock)
        {
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("18 00 00 00 01");
            //data.WriteByteArray(new byte[] { 0x95, 0x1B });
            data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            data.WriteByteArray(dec);

            foreach (Socket sockt in Program._clientSockets)
            {
                // If not on the same map, don't broadcast
                if (Program._clientPlayers[sockt.GetHashCode()].Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                try
                {
                    sockt.Send(data.getPacket());
                }
                catch { }
            }

            //sock.Send(data.getPacket()); // Oops

            Program._clientPlayers[sock.GetHashCode()].PosX = BitConverter.ToUInt16(new byte[] { dec[4], dec[5] }, 0);
            Program._clientPlayers[sock.GetHashCode()].PosY = BitConverter.ToUInt16(new byte[] { dec[6], dec[7] }, 0);

            Program.logger.Debug("Entity ID {0} moved: {1} / {2}", Program._clientPlayers[sock.GetHashCode()].EntityID, Program._clientPlayers[sock.GetHashCode()].PosX, Program._clientPlayers[sock.GetHashCode()].PosY);
        }

        public static void Sit(byte[] dec, Socket sock)
        {
            // Header (SEND and RECV):
            // 40 00 00 00 01

            // Sit SEND (initial standing) byte is   02
            // Sit RECV (initial standing) bytes are 98 1B 02

            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderByteArray(new byte[] { 0x40, 0x00, 0x00, 0x00, 0x01 });
            //data.WriteByteArray(new byte[] { 0x95, 0x1B, 0x02 });
            data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            data.WriteByte(dec[0]);
            sock.Send(data.getPacket());

            foreach (Socket sockt in Program._clientSockets)
            {
                // If not on the same map, don't broadcast
                if (Program._clientPlayers[sockt.GetHashCode()].Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                try
                {
                    sockt.Send(data.getPacket());
                }
                catch { }
            }

            Program.logger.Debug("Sit packet sent.");
        }

        public static void SitDirection(byte[] dec, Socket sock)
        {
            // 41 00 00 00 01 (Header for SEND and RECV)
            // SEND BC 02 F8 03
            // RECV 94 1B BC 02 F8 03

            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("41 00 00 00 01");
            data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            data.WriteByteArray(dec);

            sock.Send(data.getPacket());

            foreach (Socket sockt in Program._clientSockets)
            {
                // If not on the same map, don't broadcast
                if (Program._clientPlayers[sockt.GetHashCode()].Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                try
                {
                    sockt.Send(data.getPacket());
                }
                catch { }
            }
        }

        public static void ChangeZone(byte[] dec, Socket sock)
        {
            // First 4 bytes as UInt32 for user ID
            UInt32 uid = BitConverter.ToUInt32(new byte[] { dec[0], dec[1], dec[2], dec[3] }, 0);
            int orig_hash = -1;

            foreach (KeyValuePair<int, Player> entry in Program._clientPlayers)
            {
                Program.logger.Debug("Got {0}; expecting {1}.", entry.Value.ID, uid);
                if(entry.Value.ID == uid)
                {
                    Program.logger.Debug("Found {0}.", uid);
                    orig_hash = entry.Value.ClientSocket.GetHashCode();
                    Program._clientPlayers.Remove(orig_hash);
                    entry.Value.ClientSocket = sock;
                    Program._clientPlayers.Add(sock.GetHashCode(), entry.Value);
                    break;
                }
            }

            if(orig_hash == -1)
            {
                //sock.Disconnect(false);
                return;
            }

            //Program._clientPlayers[sock.GetHashCode()].EntityID = (ushort)Program._entityIdx;
            //Program._entityIdx++;

            Program._clientPlayers[sock.GetHashCode()].ClientRemoved = false;
			Program._clientPlayers[sock.GetHashCode()].ChangingMap = false;

            Program.logger.Debug("Change zone packet sent.");

            /*PacketBuffer msg1 = new PacketBuffer();
            msg1.WriteHeaderHexString("B2 01 00 00 01");
            msg1.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            msg1.WriteHeaderHexString("44 03 A0 03 E0 24 71 33 B1 24 24 24 C8 F7 5C 68 7D 68 7D 7D F0 7D 61 F1 5A F1 61 F1 5A F1 E3 0F 88 FA 7D 7D 7D 68 7D 7D 7D 7D 7D 67 7D 7D 7D 39 33 94 5C 7D B1 24 A0 D8 8E 24 24 24 C8 63 24 71 05 D6 24 24 24 C8 0D 7D 63 24 AC 22 DF 24 24 24 C8 36 0C 80 24 07 B3 FC 24 24 24 C8 7D 94 03 94 CC 47 94 94 94 66 94 B0 6E 66 94 B0 6E E8 B5 FF C7 E7 EE A4 D5 94 94 AA 94 80 24 15 15 FC 24 24 24 C8 5B 94 03 94 1D 47 94 94 94 30 4E E0 B7 30 4E E0 B7 36 66 96 C3 E7 EE A4 D5 94 94 AA 94");
            sock.Send(msg1.getPacket()); // Is that supposed to be sent? Guess not.*/

            // Get every player connected to the server
            // Send existing players to new client
            foreach (KeyValuePair<int, Player> entry in Program._clientPlayers)
            {
                // If the player selected isn't the current player
                if (entry.Key != sock.GetHashCode())
                {
                    if (entry.Value.Map == Program._clientPlayers[sock.GetHashCode()].Map)
                    {
                        Program.logger.Debug("Sending entity ID {0} to {1}", Program._clientPlayers[sock.GetHashCode()].EntityID, entry.Value.EntityID);
                        PacketBuffer msg2z = new PacketBuffer();
                        msg2z.WriteHeaderHexString("07 00 00 00 01");
                        //msg1.WriteHexString("95 1B");
                        msg2z.WriteUshort(entry.Value.EntityID);
                        msg2z.WriteHexString("01 00 01 00 00 40 00");
                        msg2z.WriteUshort(entry.Value.PosX);
                        msg2z.WriteUshort(entry.Value.PosY);
                        msg2z.WriteUshort(entry.Value.PosX);
                        msg2z.WriteUshort(entry.Value.PosY);
                        msg2z.WriteString(entry.Value.Name);
                        msg2z.WriteHexString("00 00 80 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 00");

                        sock.Send(msg2z.getPacket());
                    }
                }
            }

            // At this point, existing players don't know about this new player.
            // Get every player connected to the server
            // Send new player to existing
            foreach (KeyValuePair<int, Player> entry in Program._clientPlayers)
            {
                if (entry.Value.ClientRemoved)
                    continue;

                try
                {
                    // If the player selected isn't the current player
                    if (entry.Key != sock.GetHashCode())
                    {
                        if (entry.Value.Map == Program._clientPlayers[sock.GetHashCode()].Map)
                        {
                            Program.logger.Debug("Sending entity ID {0} to {1}", entry.Value.EntityID, Program._clientPlayers[sock.GetHashCode()].EntityID);
                            PacketBuffer msg3z = new PacketBuffer();
                            msg3z.WriteHeaderHexString("07 00 00 00 01");
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                            msg3z.WriteHexString("01 00 01 00 00 40 00");
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
                            msg3z.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
                            msg3z.WriteHexString("00 00 80 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 00");

                            entry.Value.ClientSocket.Send(msg3z.getPacket());
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    //Program._clientPlayers.Remove(entry.Key);
                    continue;
                }
            }
        }

        public static void DrillBegin(byte[] dec, Socket sock)
        {
            StringBuilder hex = new StringBuilder(dec.Length * 2);

            List<byte> packet1 = new List<byte>();

            PacketBuffer msg1_1 = new PacketBuffer();
            msg1_1.WriteHeaderHexString("2D 00 00 00 00");
            packet1.AddRange(msg1_1.getPacket());

            PacketBuffer msg1_2 = new PacketBuffer();
            msg1_2.WriteHeaderHexString("18 00 00 00 01");
            //msg1_2.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            msg1_2.WriteByteArray(dec);

            Program._clientPlayers[sock.GetHashCode()].PosX = BitConverter.ToUInt16(new byte[] { dec[0], dec[1] }, 0);
            Program._clientPlayers[sock.GetHashCode()].PosY = BitConverter.ToUInt16(new byte[] { dec[2], dec[3] }, 0);

            Program.logger.Debug("(Drill) Entity ID {0} moved: {1} ", Program._clientPlayers[sock.GetHashCode()].EntityID, Program._clientPlayers[sock.GetHashCode()].PosX + " / " + Program._clientPlayers[sock.GetHashCode()].PosY);

            /*PacketBuffer msg1_2 = new PacketBuffer();
            msg1_2.WriteHeaderHexString("18 00 00 00 01");
            msg1_2.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            msg1_2.WriteHexString("38 04 78 04 78 04 78 04 02");*/
            packet1.AddRange(msg1_2.getPacket());

            sock.Send(packet1.ToArray());

            List<byte> packet2 = new List<byte>();

            PacketBuffer msg2_1 = new PacketBuffer();
            msg2_1.WriteHeaderHexString("26 00 00 00 01");
            msg2_1.WriteHexString("64 00 00 00");
            packet2.AddRange(msg2_1.getPacket());

            PacketBuffer msg2_2 = new PacketBuffer();
            msg2_2.WriteHeaderHexString("2F 00 00 00 01");
            msg2_2.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            //msg2_2.WriteHexString("78 04 78 04 00 0A 00 01 00");
            msg2_2.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
            msg2_2.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
            msg2_2.WriteHexString("00 0A 00 01 00");
            packet2.AddRange(msg2_2.getPacket());

            PacketBuffer msg2_3 = new PacketBuffer();
            msg2_3.WriteHeaderHexString("73 01 00 00 01");
            msg2_3.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            //msg2_3.WriteHexString("78 04 78 04 00 0A 00 01 01 5B DC 00");
            msg2_3.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
            msg2_3.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
            msg2_3.WriteHexString("00 0A 00 01 01 5B DC 00");
            packet2.AddRange(msg2_3.getPacket());

            sock.Send(packet2.ToArray());

            // RECV 1
            // 090007BF2D00000000140024B2180000000172DFDB49674967496749A4
            // RECV 2
            // 0D002A09260000000162171717170092EA7301000001B01D808E808E03E103CCCC42A8031400BFCD2F00000001B6186E826E8279F9797B79

            // ... send 30 00
            // 0D007B2A300000000120202020

            // RECV 2-1
            // 0E009CD332000000015BCE87D0D00C0001F533000000015C4CF0
            // RECV 2-2
            // 1400016C1A00000001ACCDBCA241EDE8F041EDF0

            //////////////////////
            // ..... capture 2  //
            //////////////////////

            // RECV 3-1
            // 090076B02D000000001400E1A618000000015C4C32ED32ED90EDA3EDA2
            // RECV 3-2
            // 1700119773010000015C4C90EDA3EDCDC1CDF0CDAA73CD1400BCB22F000000013984AD9EC99ED082D05BD0

            // send 31 00
            // 110058623100000001CC3851AEE938CF5B

            // RECV 4-1
            // 0E0014EF3200000001A470A49A9A0C000BF5330000000116AD320D002929260000000199BABABA
        }

        public static void DrillUnk1(byte[] dec, Socket sock)
        {
            // Drop item?

            List<byte> packet1 = new List<byte>();

            PacketBuffer msg1_1 = new PacketBuffer();
            msg1_1.WriteHeaderHexString("32 00 00 00 01");
            msg1_1.WriteHexString("01 52 02 00 00");
            packet1.AddRange(msg1_1.getPacket());

            PacketBuffer msg1_2 = new PacketBuffer();
            msg1_2.WriteHeaderHexString("33 00 00 00 01");
            msg1_2.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            msg1_2.WriteHexString("01");
            packet1.AddRange(msg1_2.getPacket());

            sock.Send(packet1.ToArray());

            PacketBuffer packet2 = new PacketBuffer();
            packet2.WriteHeaderHexString("1A 00 00 00 01");
            packet2.WriteHexString("2A 00 08 02 18 04 88 01 18 04 01");

            sock.Send(packet2.getPacket());
        }

        public static void ItemEquip(byte[] dec, Socket sock)
        {
            PacketBuffer packet = new PacketBuffer();
            packet.WriteHeaderHexString("52 00 00 00 01");
            packet.WriteByteArray(dec);
            packet.WriteHexString("E4 6B 07 72 45 6B 6B 6B A6 4C 9E CD 1C"); // ?

            sock.Send(packet.getPacket());
        }
    }
}
