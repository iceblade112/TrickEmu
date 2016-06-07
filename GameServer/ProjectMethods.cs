using System;
using System.Collections.Generic;
using System.Net.Sockets;
using MySql.Data.MySqlClient;

namespace TrickEmu
{
	public class ProjectMethods
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

        public static void DisconnectPlayer(Socket disconnecting)
		{
			Program._clientPlayers[disconnecting.GetHashCode()].ClientRemoved = true;

			try
			{
				// Update char pos
				uint userId = Program._clientPlayers[disconnecting.GetHashCode()].ID;
                int mapId = Program._clientPlayers[disconnecting.GetHashCode()].Map;
                ushort xPos = Program._clientPlayers[disconnecting.GetHashCode()].PosX;
				ushort yPos =  Program._clientPlayers[disconnecting.GetHashCode()].PosY;

				using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
				{
					cmd.CommandText = "UPDATE characters SET pos_x = @xpos, pos_y = @ypos, map = @mapid WHERE id = @userid;";
					cmd.Parameters.AddWithValue("@userid", userId);
                    cmd.Parameters.AddWithValue("@mapid", mapId);
                    cmd.Parameters.AddWithValue("@xpos", xPos);
					cmd.Parameters.AddWithValue("@ypos", yPos);
					cmd.ExecuteNonQuery();
					cmd.Dispose();
				}
			} catch (Exception ex) { Program.logger.Error(ex, "Character update error."); }

			foreach (KeyValuePair<int, Player> entry in Program._clientPlayers)
			{
				if (entry.Key == disconnecting.GetHashCode()) continue;

				try
				{
					// Disconnected
					PacketBuffer dcmsg = new PacketBuffer();
					dcmsg.WriteHeaderHexString("06 00 00 00 01");
					dcmsg.WriteUshort(Program._clientPlayers[disconnecting.GetHashCode()].EntityID);

					entry.Value.ClientSocket.Send(dcmsg.getPacket());
				}
				catch
				{
					// ignored
				}
			}
		}
	}
}