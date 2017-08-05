using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TELib;

namespace TrickEmu.Packets
{
    class CharacterSitting
    {
        public static void HandleSit(Socket sock, byte[] packet)
        {
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("40 00 00 00 01");
            data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            data.WriteByte(packet[0]);
            sock.Send(data.getPacket());

            foreach (KeyValuePair<int, Character> plr in Program._clientPlayers)
            {
                // If not on the same map, don't broadcast
                if (!Program._clientPlayers.ContainsKey(plr.Value.Socket.GetHashCode()) || Program._clientPlayers[plr.Value.Socket.GetHashCode()].Map != Program._clientPlayers[plr.Value.Socket.GetHashCode()].Map) continue;

                try
                {
                    plr.Value.Socket.Send(data.getPacket());
                }
                catch { }
            }

            Program.logger.Debug("Sit packet sent.");
        }

        public static void HandleDirectionChange(Socket sock, byte[] packet)
        {
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("41 00 00 00 01");
            data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            data.WriteByteArray(packet);

            sock.Send(data.getPacket());

            foreach (KeyValuePair<int, Character> plr in Program._clientPlayers)
            {
                // If not on the same map, don't broadcast
                if (Program._clientPlayers.ContainsKey(plr.Value.Socket.GetHashCode()) && Program._clientPlayers[plr.Value.Socket.GetHashCode()].Map != Program._clientPlayers[plr.Value.Socket.GetHashCode()].Map) continue;

                try
                {
                    plr.Value.Socket.Send(data.getPacket());
                }
                catch { }
            }
        }
    }
}
