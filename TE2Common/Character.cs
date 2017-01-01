using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TE2Common
{
    public class Character
    {
        public string Username { get; set; }
        public Socket Socket { get; set; }
        public bool ClientRemoved { get; set; } = false;
        public bool ChangingMap { get; set; } = false;

        public uint ID { get; set; }
        public ushort EntityID { get; set; }
        public string Name { get; set; }
        public int Level { get; set; } = 0;
        public ulong Money { get; set; } = 0;
        public int HP { get; set; } = 100;
        public int MP { get; set; } = 80;
        public int Map { get; set; } = 1;
        public ushort PosX { get; set; } = 0;
        public ushort PosY { get; set; } = 0;

        public int Hair { get; set; }
        public int Job { get; set; }
        public int Type { get; set; }
        public int FType { get; set; }
        public string Build { get; set; }
    }
}
