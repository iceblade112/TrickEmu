using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu
{
    class MapDetails
    {
        public Dictionary<int, string> MapName = new Dictionary<int, string>();
        
        public MapDetails()
        {
            MapPos[1] = new Point { X = 200, Y = 3980 };
            MapPos[2] = new Point { X = 310, Y = 1460 };
            MapPos[3] = new Point { X = 250, Y = 440 };
            // ...
            MapPos[33] = new Point { X = 768, Y = 768 };

            MapName[1] = "Megalopolis Square";
            MapName[2] = "Gate of Oops Wharf";
            MapName[3] = "Gate of Desert Beach";
            // ...
            MapName[33] = "Beach Town - Paradise";
        }
        public Dictionary<int, Point> MapPos = new Dictionary<int, Point>();

        public struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}
