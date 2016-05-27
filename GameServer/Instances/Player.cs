using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu
{
    /// <summary>
    /// Player class
    /// Should be instantiable (that's a word, right?)  
    /// </summary>
    class Player
    {
	    // Player's socket
    	public Socket ClientSocket { get; set; }

		// Was player disposed/removed?
		public bool ClientRemoved { get; set; } = false;

		// Is player changing maps?
		public bool ChangingMap { get; set; } = false;

        // Character ID from DB
        // Set on create
        public uint ID { get; set; }
        
        // Entity ID (94 1B, 95 1B, etc)
        public ushort EntityID { get; set; }

        // Player name
        // Set on create
        public string Name { get; set; }

        // Level
        // Ha, this emulator having monsters? In your (my) dreams!
        public int Level { get; set; } = 0;

        // Galders
        // Maybe we'll eventually implement shops, etc.
        public ulong Money { get; set; } = 0;

        // HP
        public int HP { get; set; } = 100;
        
        // MP
        public int MP { get; set; } = 80;

        // Map ID (1 is Megalopolis)
        // It works!
        public int Map { get; set; } = 1;

        // X pos
        // We'll eventually get this working. Probably.
        public ushort PosX { get; set; } = 0;

        // Y pos
        // This, too.
        public ushort PosY { get; set; } = 0;
    }
}
