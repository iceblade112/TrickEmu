using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using TE2Common;

namespace TrickEmu2
{
    class GameFunctions
    {
        public static void SendChat(string text, Socket sock)
        {
            PacketBuffer data = new PacketBuffer();
            data.WriteHeaderUshort(0x13C); // Packet ID
            data.WriteHeaderByteArray(new byte[] { 0x00, 0x00, 0x01 }); // Packet header padding
            data.WriteByteArray(new byte[] { 0x00, 0x00 });
            data.WriteString(text);
            data.WriteByteArray(new byte[] { 0x00, 0x00 });

            try
            {
                sock.Send(data.getPacket());
            }
            catch { }
        }

        public static void DisconnectPlayer(Socket disconnecting)
        {
            var dcChar = Program._clientPlayers[disconnecting.GetHashCode()];

            dcChar.ClientRemoved = true;

            try
            {
                uint userId = dcChar.ID;
                int mapId = dcChar.Map;
                ushort xPos = dcChar.PosX;
                ushort yPos = dcChar.PosY;

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
            }
            catch (Exception ex) { Program.logger.Error(ex, "Character update error."); }

            foreach (KeyValuePair<int, Character> entry in Program._clientPlayers)
            {
                if (entry.Key == disconnecting.GetHashCode()) continue;

                try
                {
                    // Disconnected
                    PacketBuffer dcmsg = new PacketBuffer();
                    dcmsg.WriteHeaderHexString("06 00 00 00 01");
                    dcmsg.WriteUshort(dcChar.EntityID);

                    entry.Value.Socket.Send(dcmsg.getPacket());
                }
                catch
                {
                    // ignored
                }
            }

            Program._clientPlayers.Remove(disconnecting.GetHashCode());
        }
    }
}