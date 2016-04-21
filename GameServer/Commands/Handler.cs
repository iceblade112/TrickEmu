using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu.Commands
{
    class Handler
    {
        public static bool Handle(string chat, Socket sock)
        {
            string command = chat.Trim().Split(' ')[0];
            string[] msg = chat.Trim().Split(' ');

            if(chat.ToLower().Equals("!help"))
            {
                string[] helptext = new string[] { "TrickEmu",
                                                   "----------------",
                                                   "[Commands]",
                                                   "!oxc <text> - OX chat",
                                                   "!gmc <text> - GM chat",
                                                   "[Etc.]",
                                                   "Literally nothing.",
                                                   "...and that's about it.",};

                foreach (string s in helptext)
                {
                    PacketBuffer data = new PacketBuffer();
                    data.WriteHeaderUshort(0x13C); // Packet ID
                    data.WriteHeaderByteArray(new byte[] { 0x00, 0x00, 0x01 }); // Packet header padding
                    data.WriteByteArray(new byte[] { 0x00, 0x00 }); // ?
                    data.WriteString(s);
                    data.WriteByteArray(new byte[] { 0x00, 0x00 }); // ????

                    sock.Send(data.getPacket());
                }

                Methods.echoColor("Command Handler", ConsoleColor.DarkMagenta, "Player executed help command.");
                return true;
            } else if(chat.ToLower().Equals("!movetest"))
            {
                PacketBuffer msg1 = new PacketBuffer();
                msg1.WriteHeaderHexString("1B 00 00 00 01");
                msg1.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                msg1.WriteHexString("68 03 D3 03 02 27 CB 59 02 52 CB CB CB 8F D2 9B 9B 9B 9B 9B 74 CB 7D 15 0A CB CB CB 8F C6 5D");
                sock.Send(msg1.getPacket());

                PacketBuffer msg2 = new PacketBuffer();
                msg2.WriteHeaderHexString("9A 00 00 00 01");
                msg2.WriteHeaderHexString("DC 72 07 00 E0 1D F6 05 00 00 00 00 31 39 32 2E 31 36 38 2E 31 2E 32 36 00 F6 55 00 00 29 00 00 00 21 00 14 02 DE 02 02");
                sock.Send(msg2.getPacket());

                Methods.echoColor("Command Handler", ConsoleColor.DarkMagenta, "Player executed movetest command.");
                return true;
            }
            return false;
        }
    }
}
