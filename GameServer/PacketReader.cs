using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu
{
    class PacketReader
    {
        public enum PacketId : ushort
        {
            R_SELECTCHAR1 = 0x0400, // 0x0400
            R_SELECTCHAR2 = 0x0500,
            R_CHAT = 0x3600,
            R_SIT = 0x4000,
            R_SITDIR = 0x4100,
            R_MOVEPOS = 0x1800,
            R_HEADNOTICE = 0xB600,
            R_CHANGEZONE = 0x9C00,

            R_ITEM_EQUIP = 0x5100,
            R_DRILL_BEGIN = 0x2C00,
            R_DRILL_UNK1 = 0x3000,
            R_DRILL_UNK2 = 0x3100,
        }

        public static void handlePacket(byte[] packet, Socket sock)
        {
            int i = packet.Length;

            byte[] dec = Encryption.decrypt(packet);

            switch ((PacketId)Methods.ReadUshort(packet, 4))
            {
                case PacketId.R_SELECTCHAR1:
                    PacketWriter.SelectChar1(dec, sock);
                    break;
                case PacketId.R_SELECTCHAR2:
                    PacketWriter.SelectChar2(dec, sock);
                    break;
                case PacketId.R_CHAT:
                    PacketWriter.Chat(dec, sock);
                    break;
                case PacketId.R_MOVEPOS:
                    PacketWriter.MovePos(dec, sock);
                    break;
                case PacketId.R_SIT:
                    PacketWriter.Sit(dec, sock);
                    break;
                case PacketId.R_HEADNOTICE:
                    PacketWriter.HeadNotice(dec, sock);
                    break;
                case PacketId.R_CHANGEZONE:
                    PacketWriter.ChangeZone(dec, sock);
                    break;
                case PacketId.R_SITDIR:
                    PacketWriter.SitDirection(dec, sock);
                    break;

                case PacketId.R_ITEM_EQUIP:
                    PacketWriter.ItemEquip(dec, sock);
                    break;
                case PacketId.R_DRILL_BEGIN:
                    PacketWriter.DrillBegin(dec, sock);
                    break;
                case PacketId.R_DRILL_UNK1:
                    // Not working
                    PacketWriter.DrillUnk1(dec, sock);
                    break;
                case PacketId.R_DRILL_UNK2:
                    Console.WriteLine("Got drill unknown 2 packet.");
                    break;
                default:
                    Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, Language.strings["UnhandledPacket"]);
                    break;
            }
        }
    }
}
