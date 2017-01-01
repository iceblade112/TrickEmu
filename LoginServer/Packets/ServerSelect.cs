using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class ServerSelect
    {
        public static void Handle(Socket sock, byte[] packet)
        {
            PacketBuffer msg = new PacketBuffer(sock);

            msg.WriteHeaderHexString("F2 2C 00 00 00");

            var ip = Program.config.Server["ChannelIP"];

            var bytes = new List<byte>();
            bytes.AddRange(Encoding.ASCII.GetBytes(ip));

            for (int i = ip.Length; i < 15; i++) bytes.Add(0x00);

            if (bytes[11] == 0x00)
            {
                bytes[12] = 0x20;
                bytes[13] = 0x01;
            } else if (bytes[12] == 0x00)
            {
                bytes[13] = 0x01;
            }

            msg.WriteByteArray(bytes.ToArray()); // IP

            msg.WriteHexString("00");
            msg.WriteUshort(10006);
            msg.Send(false);
        }
    }
}
