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
            Player plr = new Player();
            plr.ID = BitConverter.ToUInt32(new byte[] { dec[0], dec[1], dec[2], dec[3] }, 0); // ... soon

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT name, level, money, health, mana FROM characters WHERE id = @userid LIMIT 1;";
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
                    }
                }
                cmd.Dispose();
            }
            Methods.echoColor("Debug", ConsoleColor.DarkBlue, plr.Name + " is entering the world.");

            Program._clientPlayers.Add(sock.GetHashCode(), plr);

            List<byte> msg = new List<byte>();
            // E6 00 00 00 01
            // Head
            PacketBuffer head = new PacketBuffer();
            head.WriteHeaderHexString("E6 00 00 00 01");
            head.WriteHexString("9E 82 07 01 95 1B D8 04 E8 06");

            // E7 00 00 00 01
            // Item
            PacketBuffer item = new PacketBuffer();
            item.WriteHeaderHexString("E7 00 00 00 01");
            item.WriteHexString("05 00 01 D6 07 EF D1 8C 77 00 00 00 00 32 00 01 9E 08 F0 D1 8C 77 00 00 00 00 32 00 02 F4 1A ED D1 8C 77 00 00 00 00 01 00 02 26 1B EE D1 8C 77 00 00 00 00 01 00 03 40 1F EC D1 8C 77 00 00 00 00 01 00 58 02");

            // E9 00 00 00 01
            // Stat
            PacketBuffer stat = new PacketBuffer();
            stat.WriteHeaderHexString("E9 00 00 00 01");
            stat.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 ED D1 8C 77 00 00 00 00 EE D1 8C 77 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 21 00 D8 04 00 00 E8 06 00 00 21 00 00 00 00 00 78 00 50 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 30 53 C9 CC E4 65 D1 01 30 53 C9 CC E4 65 D1 01 34 06 00 00 01");

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
            byte[] msg = new byte[] { 0x2F, 0x00, 0x9C, 0xF3, 0x07, 0x00, 0x00, 0x00, 0x01, 0x9B, 0x84, 0x5B, 0xD0, 0x5B, 0xD0, 0x5B, 0xB7, 0xD0, 0x95, 0x9E, 0x99, 0x5A, 0x95, 0x9E, 0x99, 0x5A, 0x6C, 0x2E, 0x53, 0xD0, 0xD0, 0xD0, 0x5B, 0xD0, 0xD0, 0xD0, 0xD0, 0xD0, 0xD8, 0xD0, 0xD0, 0xD0, 0xE7, 0x45, 0xD6, 0x84, 0x5B, 0x09, 0x00, 0x9C, 0xF3, 0x35, 0x01, 0x00, 0x00, 0x00 };
            sock.Send(msg);

            // 2?
            byte[] msg2 = new byte[] { 0x0B, 0x00, 0x52, 0x36, 0x24, 0x00, 0x00, 0x00, 0x01, 0x80, 0x03, 0x0B, 0x00, 0x41, 0x47, 0x25, 0x00, 0x00, 0x00, 0x01, 0xBF, 0xCD, 0x2C, 0x00, 0x9E, 0xAB, 0x08, 0x00, 0x00, 0x00, 0x01, 0x4A, 0xA0, 0xD8, 0xA0, 0xDC, 0x01, 0xA0, 0xA0, 0xA0, 0xC6, 0x6A, 0x3F, 0x3C, 0xC6, 0x6A, 0x3F, 0x3C, 0x23, 0x26, 0x54, 0xF5, 0x6C, 0x99, 0x84, 0xCE, 0x54, 0x36, 0xFF, 0x55, 0xA0, 0xA0, 0x05, 0xA0, 0x6A, 0xA0, 0x2E, 0x00, 0xF5, 0x22, 0x08, 0x00, 0x00, 0x00, 0x01, 0x28, 0x0F, 0x1F, 0x0F, 0xD6, 0x91, 0x0F, 0x0F, 0x0F, 0xED, 0xAB, 0x46, 0x42, 0xED, 0xAB, 0x46, 0x42, 0xA3, 0x9A, 0x94, 0xD3, 0x2D, 0x76, 0xA3, 0xF1, 0x72, 0x68, 0xD8, 0xEE, 0x7C, 0xB6, 0x0F, 0x0F, 0xBA, 0x0F, 0x64, 0x0F };
            sock.Send(msg2);

            // 3...?
            byte[] msg3 = new byte[] { 0x0C, 0x00, 0x7C, 0x33, 0x42, 0x00, 0x00, 0x00, 0x01, 0x6C, 0xD0, 0x8E };
            sock.Send(msg3);
        }

        public static void HeadNotice(byte[] dec, Socket sock)
        {
            // Head notice (personal notice)
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("B6 00 00 00 01"); // Packet header
            data.WriteByte(0x94); // ???
            data.WriteByte(0x1B); // ^
            data.WriteString(Methods.sep(Methods.getString(dec, 9), "\x00"));
            data.WriteByte(0x00);

            Console.WriteLine("Head notice text: " + Methods.sep(Methods.getString(dec, 9), "\x00"));

            sock.Send(data.getPacket());
        }

        public static void Chat(byte[] dec, Socket sock)
        {
            // Chat
            // Packet data starts at 9
            // substring 9 (10)

            string chatString = Methods.sep(Methods.getString(dec, 0), "\x00");

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
                data.WriteByteArray(new byte[] { 0x94, 0x1B }); //, 0x42, 0x6F, 0x62, 0x00 });
                data.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
                data.WriteByte(0x00);
                data.WriteString(Methods.sep(Methods.getString(dec, dec.Length), "\x00").Substring(5));
                data.WriteByte(0x00);

                byte[] newpkt = data.getPacket();

                Console.WriteLine("Sending to " + Program._clientSockets.ToArray().Length + " client(s).");

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
                data.WriteByteArray(new byte[] { 0x94, 0x1B });
                data.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
                data.WriteByte(0x00);
                data.WriteString(Methods.sep(Methods.getString(dec, dec.Length), "\x00"));
                data.WriteByte(0x00);

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

            Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, "Received chat packet: " + chatString);
        }

        public static void NotWorkingMove(byte[] dec, Socket sock)
        {
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("18 00 00 00 01");
            data.WriteByteArray(new byte[] { 0x98, 0x1B });
            data.WriteByteArray(dec);

            sock.Send(data.getPacket()); // Oops

            Console.WriteLine("Sent non-working move packet");
        }

        public static void Sit(byte[] dec, Socket sock)
        {
            // Header (SEND and RECV):
            // 40 00 00 00 01

            // Sit SEND (initial standing) byte is   02
            // Sit RECV (initial standing) bytes are 98 1B 02

            PacketBuffer data = new PacketBuffer();
            data.WriteByteArray(new byte[] { 0x40, 0x00, 0x00, 0x01 });
            data.WriteByteArray(new byte[] { 0x98, 0x1B, 0x02 });

            sock.Send(data.getPacket());

            Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, "Sit packet sent.");
        }
    }
}
