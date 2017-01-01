using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class CharacterMove
    {
        public static void Handle(Socket sock, byte[] packet)
        {
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderHexString("18 00 00 00 01");
            data.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
            data.WriteByteArray(packet);

            foreach (KeyValuePair<int, Character> val in Program._clientPlayers)
            {
                // If not on the same map, don't broadcast
                if (!Program._clientPlayers.ContainsKey(val.Value.Socket.GetHashCode()) || Program._clientPlayers[val.Value.Socket.GetHashCode()].Map != Program._clientPlayers[sock.GetHashCode()].Map) continue;

                try
                {
                    val.Value.Socket.Send(data.getPacket());
                }
                catch { }
            }

            //sock.Send(data.getPacket()); // Oops

            Program._clientPlayers[sock.GetHashCode()].PosX = BitConverter.ToUInt16(new byte[] { packet[4], packet[5] }, 0);
            Program._clientPlayers[sock.GetHashCode()].PosY = BitConverter.ToUInt16(new byte[] { packet[6], packet[7] }, 0);

            Program.logger.Debug("Entity ID {0} moved: {1} / {2}", Program._clientPlayers[sock.GetHashCode()].EntityID, Program._clientPlayers[sock.GetHashCode()].PosX, Program._clientPlayers[sock.GetHashCode()].PosY);
        }
    }
}
