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
        public static void SendChat(string chat, Socket sock)
        {
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderUshort(0x13C); // Packet ID
            data.WriteHeaderByteArray(new byte[] { 0x00, 0x00, 0x01 }); // Packet header padding
            data.WriteByteArray(new byte[] { 0x00, 0x00 }); // ?
            data.WriteString(chat);
            data.WriteByteArray(new byte[] { 0x00, 0x00 });

            try
            {
                sock.Send(data.getPacket());
            }
            catch { }
        }

        public static bool Handle(string chat, Socket sock)
        {
            string[] param = chat.Trim().Split(' ');
            string command = param[0];

            /*
            if(chat.ToLower().Equals("!help"))
            {
                
            } else if(command.Equals("!move"))
            {

                /*PacketBuffer msg2 = new PacketBuffer();
                msg2.WriteHeaderHexString("9A 00 00 00 01");
                msg2.WriteHeaderHexString("DC 72 07 00 E0 1D F6 05 00 00 00 00 31 39 32 2E 31 36 38 2E 31 2E 32 36 00 F6 55 00 00 29 00 00 00 21 00 14 02 DE 02 02");
                sock.Send(msg2.getPacket());*/

                //Methods.echoColor("Command Handler", ConsoleColor.DarkMagenta, "Player executed movetest command.");
                //return true;

            //}*/

            switch (command.Substring(1))
            {
                case "help":
                    string[] helptext = new string[] { "TrickEmu",
                                                   "----------------",
                                                   "[Commands]",
                                                   "!oxc <text> - OX chat",
                                                   "!gmc <text> - GM chat",
                                                   "!move <Map ID> - Move map",
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
                    break;
                case "move":
                    Program._clientPlayers[sock.GetHashCode()].ChangingMap = true;

                    if (param.Length < 2)
                    {
                        SendChat("Usage: !move [Map ID]", sock);
                        return true;
                    }

                    int MapId = 1;

                    try
                    {
                        MapId = int.Parse(param[1]);
                    }
                    catch
                    {
                        SendChat("Usage: !move [Map ID]", sock);
                        return true;
                    }

                    try
                    {
                        SendChat("Changing zone to " + Program.MapDetails.MapName[MapId] + ".", sock);
                    }
                    catch
                    {
                        SendChat("Changing zone to ID " + MapId + " without position data.", sock);
                    }

                    List<byte> move1 = new List<byte>();

                    PacketBuffer msg1 = new PacketBuffer();
                    msg1.WriteHeaderHexString("1B 00 00 00 01");
                    msg1.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                    msg1.WriteHexString("A4 05 5C 00 02");
                    move1.AddRange(msg1.getPacket());

                    PacketBuffer msg2 = new PacketBuffer();
                    msg2.WriteHeaderHexString("99 00 00 00 01");
                    msg2.WriteByte((byte)MapId);
                    msg2.WriteHexString("00 00 00 00 00");
                    move1.AddRange(msg2.getPacket());

                    PacketBuffer msg3 = new PacketBuffer();
                    msg3.WriteHeaderHexString("15 00 00 00 01");
                    msg3.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                    move1.AddRange(msg3.getPacket());

                    sock.Send(move1.ToArray());

                    PacketBuffer pkt2 = new PacketBuffer();
                    pkt2.WriteHeaderHexString("9A 00 00 00 01");
                    pkt2.WriteHexString("F2 92 B3 00");
                    pkt2.WriteUInt32(Program._clientPlayers[sock.GetHashCode()].ID);
                    pkt2.WriteHexString("00 00 00 00 31 32 37 2E 30 2E 30 2E 31 00 F6 55 00 00 23 48 00 00");
                    pkt2.WriteByte((byte)MapId);
                    pkt2.WriteHexString("00");

                    if (param.Length > 2)
                    {
                        Console.WriteLine("Length of param: " + param.Length);
                        try
                        {
                            pkt2.WriteUshort((ushort)int.Parse(param[2]));
                        }
                        catch
                        {
                            SendChat("Invalid X position given. Using default X 512.", sock);
                            pkt2.WriteUshort((ushort)512);
                        }
                    }
                    else {
                        try
                        {
                            pkt2.WriteUshort((ushort)Program.MapDetails.MapPos[MapId].X); // X pos
                        }
                        catch
                        {
                            pkt2.WriteUshort((ushort)512);
                        }
                    }

                    if (param.Length > 3)
                    {
                        Console.WriteLine("Length of param: " + param.Length);
                        try
                        {
                            pkt2.WriteUshort((ushort)int.Parse(param[3]));
                        }
                        catch
                        {
                            SendChat("Invalid Y position given. Using default Y 512.", sock);
                            pkt2.WriteUshort((ushort)512);
                        }
                    }
                    else {
                        try
                        {
                            pkt2.WriteUshort((ushort)Program.MapDetails.MapPos[MapId].Y); // Y pos
                        }
                        catch
                        {
                            pkt2.WriteUshort((ushort)512);
                        }
                    }

                    pkt2.WriteHexString("02");
                    sock.Send(pkt2.getPacket());
                    return true;
                    break;
            }

            return false;
        }
    }
}
