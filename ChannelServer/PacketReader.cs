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
            R_ALIVEPING = 0xDF07,
            R_REQCHARSEL1 = 0xD007,
            R_REQCHARSEL2 = 0xDC07,
        }

        public static void handlePacket(byte[] packet, Socket sock)
        {
            int i = packet.Length;

            byte[] dec = Encryption.decrypt(packet);

            switch ((PacketId)Methods.ReadUshort(packet, 4))
            {
                case PacketId.R_REQCHARSEL1:
                    PacketWriter.ReqCharSelect(dec, sock);
                    break;
                case PacketId.R_REQCHARSEL2:
                    PacketWriter.SelectCharIngame(dec, sock);
                    break;
                case PacketId.R_ALIVEPING:
                    break;
                default:
                    Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, Language.strings["UnhandledPacket"]);
                    break;
            }
            
        }
    }
}
