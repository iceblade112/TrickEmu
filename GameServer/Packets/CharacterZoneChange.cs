using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TELib;

namespace TrickEmu.Packets
{
    class CharacterZoneChange
    {
        public static void Handle(Socket sock, byte[] packet)
        {
            // First 4 bytes as UInt32 for user ID
            UInt32 uid = BitConverter.ToUInt32(packet, 0);
            int orig_hash = -1;

            foreach (KeyValuePair<int, Character> entry in Program._clientPlayers)
            {
                Program.logger.Debug("Got {0}; expecting {1}.", entry.Value.ID, uid);
                if (entry.Value.ID == uid)
                {
                    Program.logger.Debug("Found {0}.", uid);
                    orig_hash = entry.Value.Socket.GetHashCode();
                    Program._clientPlayers.Remove(orig_hash);
                    entry.Value.Socket = sock;
                    Program._clientPlayers.Add(sock.GetHashCode(), entry.Value);
                    break;
                }
            }

            if (orig_hash == -1)
            {
                Program.logger.Warn("Player ID not found.");
                return;
            }

            Program._clientPlayers[sock.GetHashCode()].ClientRemoved = false;
            Program._clientPlayers[sock.GetHashCode()].ChangingMap = false;

            Program.logger.Debug("Change zone packet sent.");
            
            // Get every player connected to the server
            // Send existing players to new client
            foreach (KeyValuePair<int, Character> entry in Program._clientPlayers)
            {
                // If the player selected isn't the current player
                if (entry.Key != sock.GetHashCode())
                {
                    if (entry.Value.Map == Program._clientPlayers[sock.GetHashCode()].Map)
                    {
                        Program.logger.Debug("Sending entity ID {0} to {1}", Program._clientPlayers[sock.GetHashCode()].EntityID, entry.Value.EntityID);
                        PacketBuffer msg2z = new PacketBuffer();
                        msg2z.WriteHeaderHexString("07 00 00 00 01");
                        msg2z.WriteUshort(entry.Value.EntityID);
                        msg2z.WriteHexString("01 00 01 00 00 40 00");
                        msg2z.WriteUshort(entry.Value.PosX);
                        msg2z.WriteUshort(entry.Value.PosY);
                        msg2z.WriteUshort(entry.Value.PosX);
                        msg2z.WriteUshort(entry.Value.PosY);
                        msg2z.WriteString(entry.Value.Name);
                        msg2z.WriteHexString("00 00 80 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 00");

                        sock.Send(msg2z.getPacket());
                    }
                }
            }

            // At this point, existing players don't know about this new player.
            // Get every player connected to the server
            // Send new player to existing
            foreach (KeyValuePair<int, Character> entry in Program._clientPlayers)
            {
                if (entry.Value.ClientRemoved)
                    continue;

                try
                {
                    // If the player selected isn't the current player
                    if (entry.Key != sock.GetHashCode())
                    {
                        if (entry.Value.Map == Program._clientPlayers[sock.GetHashCode()].Map)
                        {
                            Program.logger.Debug("Sending entity ID {0} to {1}", entry.Value.EntityID, Program._clientPlayers[sock.GetHashCode()].EntityID);
                            PacketBuffer msg3z = new PacketBuffer();
                            msg3z.WriteHeaderHexString("07 00 00 00 01");
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].EntityID);
                            msg3z.WriteHexString("01 00 01 00 00 40 00");
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosX);
                            msg3z.WriteUshort(Program._clientPlayers[sock.GetHashCode()].PosY);
                            msg3z.WriteString(Program._clientPlayers[sock.GetHashCode()].Name);
                            msg3z.WriteHexString("00 00 80 01 00 00 00 00 00 60 00 00 00 F4 1A 26 1B 00");

                            entry.Value.Socket.Send(msg3z.getPacket());
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                    continue;
                }
            }
        }
    }
}
