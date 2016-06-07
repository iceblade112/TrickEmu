using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using MySql.Data.MySqlClient;
using NLog;

namespace TrickEmu
{
    class Program
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        private static Socket _serverSocket;
        public static readonly List<Socket> _clientSockets = new List<Socket>();
        private const int _BUFFER_SIZE = 2048;
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];
		public static Dictionary<int, Player> _clientPlayers = new Dictionary<int, Player>();
        public static Dictionary<int, ushort> _entityIDs = new Dictionary<int, ushort>();
        public static int _entityIdx = 7060;

        public static string _GameIP = "127.0.0.1";
        private static string _SLang = "en";
        private static string _MySQLUser = "root";
        private static string _MySQLPass = "root";
        private static string _MySQLHost = "127.0.0.1";
        private static string _MySQLPort = "3306";
        private static string _MySQLDB = "trickemu";
        public static MySqlConnection _MySQLConn;
        public static string[] _MOTD;

        public static MapDetails MapDetails = new MapDetails();

        static void Main(string[] args)
        {
            new Language(); // Initialize default language strings
            Console.Title = "TrickEmu Game (0.50)";

            if (!File.Exists("TESettings.cfg"))
            {
                logger.Info(Language.strings["ConfigNotExist"]);
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
                    ini.Write("GameIP", "127.0.0.1");
                    ini.Save();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, Language.strings["ConfigErrorUseDefault"]);
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

                    if (!inifile.KeyExists("GameIP"))
                    {
                        inifile.Write("GameIP", "127.0.0.1");
                    }
                    else
                    {
                        _GameIP = inifile.Read("GameIP");
                    }

                    inifile.Save();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, Language.strings["ConfigErrorUseDefault"]);
                }

                // Language
                if (_SLang != "en")
                {
                    try {
                        Language.loadFromFile(_SLang);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Could not load language: ");
                    }
                }

                _MOTD = new string[] { Language.strings["MOTDTEWelcome"] };

                // MOTD
                if (!File.Exists("MOTD.txt"))
                {
                    try
                    {
                        File.WriteAllText("MOTD.txt", "Welcome to TrickEmu!");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, Language.strings["MOTDCreateError"]);
                    }
                } else
                {
                    try
                    {
                        _MOTD = File.ReadAllText("MOTD.txt").Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, Language.strings["MOTDLoadError"]);
                    }
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
                logger.Error(ex, Language.strings["MySQLConnectError"]);
                Console.ReadKey();
                Environment.Exit(1); // Exit with error code 1 because error
            }

            logger.Info(Language.strings["StartingServer"]);
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Config.gamePort));
                _serverSocket.Listen(5);
                _serverSocket.BeginAccept(AcceptCallback, null);
                logger.Info(Language.strings["StartedServer"], Config.gamePort.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex, Language.strings["ErrorStartServer"]);
            }

			new Timer(DisconnectTimer, null, 0, 1000);

            while (true) Console.ReadLine();
        }

		private static void DisconnectTimer(Object o) {
			List<int> disposedPlayers = new List<int>();

			foreach (KeyValuePair<int, Player> entry in Program._clientPlayers)
			{
				bool disposed = false;
				try {
					if(entry.Value.ClientSocket.Connected) {
						disposed = false;
					}
					if(entry.Value.ClientRemoved && !entry.Value.ChangingMap) {
						disposed = true;
					}
				}
				catch (ObjectDisposedException) {
					disposed = true;
				}

				if (disposed == true) {
					disposedPlayers.Add(entry.Key);
				}
			}


			foreach (int key in disposedPlayers) {
				foreach (KeyValuePair<int, Player> plr in Program._clientPlayers) {
					if (plr.Key == key) {
						continue;
					}

					try {
						// Disconnected
						PacketBuffer dcmsg = new PacketBuffer ();
						dcmsg.WriteHeaderHexString ("06 00 00 00 01");
						dcmsg.WriteUshort (Program._clientPlayers[key].EntityID);

						plr.Value.ClientSocket.Send(dcmsg.getPacket());
					} catch {
						// ignored
					}
				}

				logger.Debug("Removing disposed entity {0}.", Program._clientPlayers[key].EntityID);

				Program._clientPlayers.Remove(key);
			}

			GC.Collect();
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
            logger.Info(Language.strings["ClientAccepted"], socket.RemoteEndPoint.ToString());
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);

				// Check if disconnected
				if(!current.Connected || received == 0)
				{
					ProjectMethods.DisconnectPlayer(current);

					logger.Info(Language.strings["GracefulDisconnect"], current.RemoteEndPoint.ToString());
					_entityIDs.Remove(current.GetHashCode());
					_clientSockets.Remove(current);
					current.Close();
					return;
				}
            }
            catch (SocketException)
            {
                logger.Warn(Language.strings["ForcefulDisconnect"], current.RemoteEndPoint.ToString());

                ProjectMethods.DisconnectPlayer(current);

                _entityIDs.Remove(current.GetHashCode());
				_clientPlayers.Remove(current.GetHashCode());
				_clientSockets.Remove(current);
				current.Close();
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);

            if (received != 0)
            {
                try {
                    PacketReader.handlePacket(recBuf, current);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, Language.strings["MalformedPacketError"]);
                }
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
