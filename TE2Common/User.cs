using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TE2Common
{
    public class User
    {
        public string Username { get; set; }
        public string Version { get; set; }
        public Socket Socket { get; set; }
    }
}
