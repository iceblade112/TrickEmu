using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu
{
    public class Config
    {
        /// <summary>
        /// Encoding to use for getting things.
        /// Guobiao encoding is used for 0.50 because "China".
        /// </summary>
        public static Encoding encoding = Encoding.GetEncoding("gb2312");

        /// <summary>
        /// LoginServer port (FLS)
        /// </summary>
        public static int loginPort = 14446;

        /// <summary>
        /// ChannelServer port (LS)
        /// </summary>
        public static int channelPort = 10006;

        /// <summary>
        /// GameServer port (GS)
        /// </summary>
        public static int gamePort = 22006;
    }
}
