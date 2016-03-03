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
            R_LOGIN = 0xED2C,
            R_SERVERSEL = 0xF12C,
        }

        public static void handlePacket(byte[] packet, Socket sock)
        {
            int i = packet.Length;

            byte[] dec = Encryption.decrypt(packet);

            switch ((PacketId)Methods.ReadUshort(packet, 4))
            {
                case PacketId.R_LOGIN:
                    PacketWriter.Login(dec, sock);
                    break;
                case PacketId.R_SERVERSEL:
                    PacketWriter.SelectServer(dec, sock);
                    break;
                default:
                    Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, Language.strings["UnhandledPacket"]);
                    break;
            }
        }
    }
}
