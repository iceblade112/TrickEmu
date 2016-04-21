using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using MySql.Data.MySqlClient;

namespace TrickEmu
{
    class Program
    {
        private static Socket _serverSocket;
        public static readonly List<Socket> _clientSockets = new List<Socket>();
        private const int _BUFFER_SIZE = 2048;
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];
        public static Dictionary<int, Player> _clientPlayers = new Dictionary<int, Player>();
        public static Dictionary<int, ushort> _entityIDs = new Dictionary<int, ushort>();
        public static int _entityIdx = 7060;

        private static string _SLang = "en";
        private static string _MySQLUser = "root";
        private static string _MySQLPass = "root";
        private static string _MySQLHost = "127.0.0.1";
        private static string _MySQLPort = "3306";
        private static string _MySQLDB = "trickemu";
        public static MySqlConnection _MySQLConn;

        // TO-DO: Player instance class

        static void Main(string[] args)
        {
            new Language(); // Initialize default language strings
            Console.Title = "TrickEmu Game (0.50)";

            if (!File.Exists("TESettings.cfg"))
            {
                Methods.echoColor(Language.strings["General"], ConsoleColor.DarkCyan, Language.strings["ConfigNotExist"]);
                try
                {
                    var inifile = File.Create("TESettings.cfg");
                    inifile.Close();
                    var ini = new ConfigReader("TESettings.cfg");
                    ini.Write("Language", "en");
                    ini.Write("User", "root");
                    ini.Write("Pass", "root");
                    ini.Write("Host", "127.0.0.1");
                    ini.Write("Port", "3306");
                    ini.Write("DB", "trickemu");
                    ini.Save();
                }
                catch (Exception ex)
                {
                    Methods.echoColor(Language.strings["General"], ConsoleColor.DarkCyan, Language.strings["ConfigErrorUseDefault"]);
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                try
                {
                    var inifile = new ConfigReader("TESettings.cfg");

                    if (!inifile.KeyExists("Language"))
                    {
                        inifile.Write("Language", "en");
                    }
                    else
                    {
                        _SLang = inifile.Read("Language");
                    }

                    if (!inifile.KeyExists("User"))
                    {
                        inifile.Write("User", "root");
                    }
                    else
                    {
                        _MySQLUser = inifile.Read("User");
                    }

                    if (!inifile.KeyExists("Pass"))
                    {
                        inifile.Write("Pass", "root");
                    }
                    else
                    {
                        _MySQLPass = inifile.Read("Pass");
                    }

                    if (!inifile.KeyExists("Host"))
                    {
                        inifile.Write("Host", "127.0.0.1");
                    }
                    else
                    {
                        _MySQLHost = inifile.Read("Host");
                    }

                    if (!inifile.KeyExists("Port"))
                    {
                        inifile.Write("User", "3306");
                    }
                    else
                    {
                        _MySQLPort = inifile.Read("Port");
                    }

                    if (!inifile.KeyExists("DB"))
                    {
                        inifile.Write("DB", "trickemu");
                    }
                    else
                    {
                        _MySQLDB = inifile.Read("DB");
                    }
                }
                catch (Exception ex)
                {
                    Methods.echoColor(Language.strings["General"], ConsoleColor.DarkCyan, Language.strings["ConfigErrorUseDefault"]);
                    Console.WriteLine(ex);
                }

                // Language
                if (_SLang != "en")
                {
                    try {
                        Language.loadFromFile(_SLang);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not load language: " + ex);
                    }
                }

                // MySQL
                _MySQLConn = new MySqlConnection("server=" + _MySQLHost + ";port=" + _MySQLPort + ";database=" + _MySQLDB + ";uid=" + _MySQLUser + ";pwd=" + _MySQLPass + ";");
                try
                {
                    _MySQLConn.Open();
                }
                catch (Exception ex)
                {
                    Methods.echoColor(Language.strings["General"], ConsoleColor.DarkCyan, Language.strings["MySQLConnectError"]);
                    Console.WriteLine(ex);
                    Console.ReadKey();
                    Environment.Exit(1); // Exit with error code 1 because error
                }
            }
            
            Methods.echoColor(Language.strings["SocketSys"], ConsoleColor.DarkGreen, Language.strings["StartingServer"]);
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Config.gamePort));
                _serverSocket.Listen(5);
                _serverSocket.BeginAccept(AcceptCallback, null);
                Methods.echoColor(Language.strings["SocketSys"], ConsoleColor.DarkGreen, Language.strings["StartedServer"], new string[] { Config.channelPort.ToString() });
            }
            catch (Exception ex)
            {
                Console.WriteLine(Language.strings["ErrorStartServer"] + ex);
            }
            while (true) Console.ReadLine();
        }

        private static void CloseAllSockets()
        {
            foreach (Socket socket in _clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            _serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = _serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            _clientSockets.Add(socket);
            socket.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Methods.echoColor(Language.strings["SocketSys"], ConsoleColor.DarkGreen, Language.strings["ClientAccepted"], new string[] { socket.RemoteEndPoint.ToString() });
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Methods.echoColor(Language.strings["SocketSys"], ConsoleColor.DarkGreen, Language.strings["ForcefulDisconnect"], new string[] { current.RemoteEndPoint.ToString() });
                current.Close();
                _clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);

            if (received != 0)
            {
                PacketReader.handlePacket(recBuf, current);
            }
            else
            {
                return;
            }

            current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            return;
        }
    }
}
