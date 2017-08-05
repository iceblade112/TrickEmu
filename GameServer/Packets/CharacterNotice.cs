using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu.Packets
{
    class CharacterNotice
    {
        public static void HandleHead(Socket sock, byte[] packet)
        {
            // Head notice (personal notice)
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("B6 00 00 00 01"); // Packet header
            //data.WriteByte(0x95); // ???
            //data.WriteByte(0x1B); // ^
            data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            data.WriteString(Methods.sep(Methods.getString(packet, 9), "\x00"));
            data.WriteByte(0x00);

            Program.logger.Debug("Head notice text: {0}", Methods.sep(Methods.getString(packet, 9), "\x00"));

            sock.Send(data.getPacket());

            foreach (KeyValuePair<int, Character> plr in Program._clientPlayers)
            {
                // If not on the same map, don't broadcast
                if (!Program._clientPlayers.ContainsKey(plr.Value.Socket.GetHashCode()) || Program._clientPlayers[plr.Value.Socket.GetHashCode()].Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                try
                {
                    plr.Value.Socket.Send(data.getPacket());
                }
                catch { }
            }
        }
    }
}
