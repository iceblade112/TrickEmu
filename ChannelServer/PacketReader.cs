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
            R_REQCHARSEL = 0xD007,
            R_NEWCHAR = 0xD607,
            R_SELCHARIG = 0xDC07,
        }

        public static void handlePacket(byte[] packet, Socket sock)
        {
            int i = packet.Length;

            byte[] dec = Encryption.decrypt(packet);

            switch ((PacketId)Methods.ReadUshort(packet, 4))
            {
                case PacketId.R_REQCHARSEL:
                    PacketWriter.ReqCharSelect(dec, sock);
                    break;
                case PacketId.R_SELCHARIG:
                    PacketWriter.SelectCharIngame(dec, sock);
                    break;
                case PacketId.R_NEWCHAR:
                    PacketWriter.CreateCharacter(dec, sock);
                    break;
                case PacketId.R_ALIVEPING:
                    break;
                default:
                    Program.logger.Warn(Language.strings["UnhandledPacket"]);
                    break;
            }
        }
    }
}
