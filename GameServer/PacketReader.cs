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
            R_SELECTCHAR2 = 0x0900,
            R_CHAT = 0x3600,
            R_SIT = 0x4000,
            R_HEADNOTICE = 0xB600,
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
                case PacketId.R_SIT:
                    PacketWriter.Sit(dec, sock);
                    break;
                case PacketId.R_HEADNOTICE:
                    PacketWriter.HeadNotice(dec, sock);
                    break;
                default:
                    Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, Language.strings["UnhandledPacket"]);
                    break;
            }
        }
    }
}
