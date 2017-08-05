using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TELib;

namespace TrickEmu.Packets
{
    class CharacterChat
    {
        public static void HandleChat(Socket sock, byte[] packet)
        {
            string chatString = Methods.sep(Methods.getString(packet, 0), "\x00");
            
            if (Commands.Handle(sock, chatString)) return;

            if (chatString.StartsWith("!gmc "))
            {
                // GM chat
                PacketBuffer data = new PacketBuffer();
                data.WriteHeaderUshort(0x13C); // Packet ID
                data.WriteHeaderByteArray(new byte[] { 0x00, 0x00, 0x01 }); // Packet header padding
                data.WriteByteArray(new byte[] { 0x00, 0x00 }); // ?
                data.WriteString(Methods.sep(Methods.getString(packet, 0), "\x00").Substring(5)); // Actual chat message
                data.WriteByteArray(new byte[] { 0x00, 0x00 }); // ????

                byte[] newpkt = data.getPacket();

                foreach (KeyValuePair<int, Character> plr in Program._clientPlayers)
                {
                    if (!Program._clientPlayers.ContainsKey(plr.Value.Socket.GetHashCode())) continue;

                    try
                    {
                        plr.Value.Socket.Send(newpkt);
                    }
                    catch { }
                }
            }
            else if (chatString.StartsWith("!oxc "))
            {
                // Normal chat
                PacketBuffer data = new PacketBuffer();
                data.WriteHeaderByteArray(new byte[] { 0xB5, 0x00, 0x00, 0x00, 0x01 });
                data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                data.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
                data.WriteByte(0x00);
                data.WriteString(Methods.sep(Methods.getString(packet, packet.Length), "\x00").Substring(5));
                data.WriteByte(0x00);

                byte[] newpkt = data.getPacket();

                Program.logger.Debug("Sending to {0} client(s).", Program._clientPlayers.ToArray().Length);

                foreach (KeyValuePair<int, Character> plr in Program._clientPlayers)
                {
                    if (!Program._clientPlayers.ContainsKey(plr.Value.Socket.GetHashCode())) continue;

                    try
                    {
                        plr.Value.Socket.Send(newpkt);
                    }
                    catch { }
                }
            }
            else
            {
                // Normal chat
                PacketBuffer data = new PacketBuffer();
                data.WriteHeaderByteArray(new byte[] { 0x39, 0x00, 0x00, 0x00, 0x01 }); // Header (0x39 ID)
                data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                data.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
                data.WriteByte(0x00);
                data.WriteString(Methods.sep(Methods.getString(packet, packet.Length), "\x00"));
                data.WriteByte(0x00);

                byte[] newpkt = data.getPacket();

                foreach (KeyValuePair<int, Character> plr in Program._clientPlayers)
                {
                    try
                    {
                        // If not on the same map, don't broadcast
                        if (!Program._clientPlayers.ContainsKey(plr.Value.Socket.GetHashCode()) || Program._clientPlayers[plr.Value.Socket.GetHashCode()].Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;
                        
                        plr.Value.Socket.Send(newpkt);
                    }
                    catch { }
                }
            }

            Program.logger.Debug("Received chat packet from entity {0}: {1}", Program._clientPlayers[sock.GetHashCode()].EntityID, chatString);
        }

        public static void HandleEmote(Socket sock, byte[] packet)
        {
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("3E 00 00 00 01");
            data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            data.WriteByte(packet[0]);

            byte[] newpkt = data.getPacket();

            foreach (KeyValuePair<int, Character> plr in Program._clientPlayers)
            {
                try
                {
                    // If not on the same map, don't broadcast
                    if (!Program._clientPlayers.ContainsKey(plr.Value.Socket.GetHashCode()) || Program._clientPlayers[plr.Value.Socket.GetHashCode()].Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                    if (plr.Value.Socket != sock)
                    {
                        plr.Value.Socket.Send(newpkt);
                    }
                }
                catch { }
            }

            sock.Send(newpkt);
        }
    }
}
