using System;
using System.Collections.Generic;
using System.Linq;
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
        // Character ID from DB
        // Set on create
        public UInt32 ID { get; set; }

        // Player name
        // Set on create
        public string Name { get; set; }

        // Level
        // Ha, this emulator having monsters? In your (my) dreams!
        public int Level { get; set; } = 0;

        // Galders
        // Maybe we'll eventually implement shops, etc.
        public UInt64 Money { get; set; } = 0;

        // HP
        public int HP { get; set; } = 100;
        
        // MP
        public int MP { get; set; } = 80;

        // Map ID (1 is Megalopolis)
        // We'll eventually get this working. EVENTUALLY.
        public int Map { get; set; } = 1;

        // X pos
        // We'll also eventually get this working. Probably.

    }
}
