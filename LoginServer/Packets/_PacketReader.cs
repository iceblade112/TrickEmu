using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TELib;

namespace TrickEmu.Packets
{
    class _PacketReader
    {
        public static void HandlePacket(Socket sock, byte[] packet)
        {
            int length = packet.Length;

            byte[] dec = Encryption.decrypt(packet);

            switch (Methods.ReadUshort(packet, 4))
            {
                // Login
                case 0xED2C:
                    Login.Handle(sock, dec);
                    break;
                // Server select
                case 0xF12C:
                    ServerSelect.Handle(sock, dec);
                    break;
                default:
                    Program.logger.Warn("Unhandled packet received.");
                    break;
            }
        }
    }
}
