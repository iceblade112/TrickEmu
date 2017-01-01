using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class _PacketReader
    {
        public static void HandlePacket(Socket sock, byte[] packet)
        {
            int length = packet.Length;

            byte[] dec = Encryption.decrypt(packet);

            switch (Methods.ReadUshort(packet, 4))
            {
                // Character selection 1
                case 0x0400:
                    CharacterSelect.SelectChar1(sock, dec);
                    break;

                // Character selection 2
                case 0x0500:
                    CharacterSelect.SelectChar2(sock, dec);
                    break;

                // Move position
                case 0x1800:
                    // move
                    CharacterMove.Handle(sock, dec);
                    break;

                // Chat
                case 0x3600:
                    CharacterChat.HandleChat(sock, dec);
                    break;

                // Chat emote
                case 0x3E00:
                    CharacterChat.HandleEmote(sock, dec);
                    break;

                // Head notice
                case 0xB600:
                    CharacterNotice.HandleHead(sock, dec);
                    break;

                // Sitting
                case 0x4000:
                    CharacterSitting.HandleSit(sock, dec);
                    break;

                // Sit direction change
                case 0x4100:
                    CharacterSitting.HandleDirectionChange(sock, dec);
                    break;

                // Zone change
                case 0x9C00:
                    CharacterZoneChange.Handle(sock, dec);
                    break;

                default:
                    Program.logger.Warn("Unhandled packet received.");
                    break;
            }
        }
    }
}
