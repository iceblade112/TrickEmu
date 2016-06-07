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
            string[] param = chat.Trim().Split(' ');
            string command = param[0];

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

                    Program.logger.Debug("Player executed help command.");
                    return true;
                case "move":
                    Program._clientPlayers[sock.GetHashCode()].ChangingMap = true;

                    if (param.Length < 2)
                    {
                        ProjectMethods.SendChat("Usage: !move [Map ID]", sock);
                        return true;
                    }

                    int MapId = 1;

                    try
                    {
                        MapId = int.Parse(param[1]);
                    }
                    catch
                    {
                        ProjectMethods.SendChat("Usage: !move [Map ID]", sock);
                        return true;
                    }

                    try
                    {
                        ProjectMethods.SendChat("Changing zone to " + Program.MapDetails.MapName[MapId] + ".", sock);
                    }
                    catch
                    {
                        ProjectMethods.SendChat("Changing zone to ID " + MapId + " without position data.", sock);
                    }

                    foreach (KeyValuePair<int, Player> entry in Program._clientPlayers)
                    {
                        if (entry.Key == sock.GetHashCode()) continue;
                        // Map value hasn't been changed yet for the moving player.
                        // But if the looped player isn't in the map, there's no need to broadcast this to them.
                        if (entry.Value.Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                        try
                        {
                            // Disconnected
                            PacketBuffer dcmsg = new PacketBuffer();
                            dcmsg.WriteHeaderHexString("06 00 00 00 01");
                            dcmsg.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);

                            entry.Value.ClientSocket.Send(dcmsg.getPacket());
                        }
                        catch
                        {
                            // ignored
                        }
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
                    pkt2.WriteHexString("00 00 00 00");
                    pkt2.WriteString(Program._GameIP);
                    pkt2.WriteHexString("00 F6 55 00 00 23 48 00 00");
                    pkt2.WriteByte((byte)MapId);
                    pkt2.WriteHexString("00");

                    int PosX = 512;
                    int PosY = 512;

                    // X coordinate
                    if (param.Length > 2)
                    {
                        Program.logger.Debug("Length of param: {0}", param.Length);
                        try
                        {
                            pkt2.WriteUshort((ushort)int.Parse(param[2]));
                            PosX = int.Parse(param[2]);
                        }
                        catch
                        {
                            ProjectMethods.SendChat("Invalid X position given. Using default X 512.", sock);
                            pkt2.WriteUshort((ushort)PosX);
                        }
                    }
                    else {
                        try
                        {
                            pkt2.WriteUshort((ushort)Program.MapDetails.MapPos[MapId].X); // X pos
                            PosX = Program.MapDetails.MapPos[MapId].X;
                        }
                        catch
                        {
                            pkt2.WriteUshort((ushort)PosX);
                        }
                    }

                    // Y coordinate
                    if (param.Length > 3)
                    {
                        Program.logger.Debug("Length of param: {0}", param.Length);
                        try
                        {
                            pkt2.WriteUshort((ushort)int.Parse(param[3]));
                            PosY = int.Parse(param[3]);
                        }
                        catch
                        {
                            ProjectMethods.SendChat("Invalid Y position given. Using default Y 512.", sock);
                            pkt2.WriteUshort((ushort)PosY);
                        }
                    }
                    else {
                        try
                        {
                            pkt2.WriteUshort((ushort)Program.MapDetails.MapPos[MapId].Y); // Y pos
                            PosY = Program.MapDetails.MapPos[MapId].Y;
                        }
                        catch
                        {
                            pkt2.WriteUshort((ushort)PosY);
                        }
                    }

                    Program._clientPlayers[sock.GetHashCode()].Map = MapId;
                    Program._clientPlayers[sock.GetHashCode()].PosX = (ushort)PosX;
                    Program._clientPlayers[sock.GetHashCode()].PosY = (ushort)PosY;

                    pkt2.WriteHexString("02");
                    sock.Send(pkt2.getPacket());
                    return true;
            }

            return false;
        }
    }
}
