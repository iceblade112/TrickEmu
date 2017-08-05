using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu.Packets
{
    class _PacketReader
    {
        public static void HandlePacket(Socket sock, byte[] packet)
        {
            int length = packet.Length;

            byte[] dec = Encryption.decrypt(packet);

            Program.logger.Debug("Packet: " + BitConverter.ToString(dec).Replace("-", " "));

            switch (Methods.ReadUshort(packet, 4))
            {
                // Character list
                case 0xD007:
                    CharacterList.Handle(sock, dec);
                    break;
                // Select character
                case 0xDC07:
                    SelectCharacter.Handle(sock, dec);
                    break;
                // New character
                case 0xD607:
                    CreateCharacter.Handle(sock, dec);
                    break;
                // Delete character after confirmation
                case 0xD907:
                    DeleteCharacter.Handle(sock, dec);
                    break;
                // Keep-alive ping packet
                case 0xDF07:
                    break;
                default:
                    Program.logger.Warn("Unhandled packet received.");
                    break;
            }
        }
    }
}
