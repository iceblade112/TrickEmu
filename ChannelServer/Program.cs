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
        private static readonly List<Socket> _clientSockets = new List<Socket>();
        private const int _BUFFER_SIZE = 2048;
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];

        private static string _SLang = "en";
        private static string _MySQLUser = "root";
        private static string _MySQLPass = "root";
        private static string _MySQLHost = "127.0.0.1";
        private static string _MySQLPort = "3306";
        private static string _MySQLDB = "trickemu";
        private static MySqlConnection _MySQLConn;

        static void Main(string[] args)
        {
            new Language(); // Initialize default language strings
            Console.Title = "TrickEmu Channel (0.50)";

            if (!File.Exists("TESettings.ini"))
            {
                Methods.echoColor(Language.strings["General"], ConsoleColor.DarkCyan, Language.strings["ConfigNotExist"]);
                try
                {
                    var inifile = File.Create("TESettings.ini");
                    inifile.Close();
                    var ini = new ConfigReader("TESettings.ini");
                    ini.Write("Language", "en", "General");
                    ini.Write("User", "root", "MySQL");
                    ini.Write("Pass", "root", "MySQL");
                    ini.Write("Host", "127.0.0.1", "MySQL");
                    ini.Write("Port", "3306", "MySQL");
                    ini.Write("DB", "trickemu", "MySQL");
                }
                catch (Exception ex)
                {
                    Methods.echoColor(Language.strings["General"], ConsoleColor.DarkCyan, Language.strings["ConfigErrorUseDefault"]);
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                try {
                    var inifile = new ConfigReader("TESettings.ini");

                    if (!inifile.KeyExists("Language", "General"))
                    {
                        inifile.Write("Language", "en", "General");
                    }
                    else
                    {
                        _SLang = inifile.Read("Language", "General");
                    }

                    if (!inifile.KeyExists("User", "MySQL"))
                    {
                        inifile.Write("User", "root", "MySQL");
                    } else
                    {
                        _MySQLUser = inifile.Read("User", "MySQL");
                    }

                    if (!inifile.KeyExists("Pass", "MySQL"))
                    {
                        inifile.Write("Pass", "root", "MySQL");
                    }
                    else
                    {
                        _MySQLPass = inifile.Read("Pass", "MySQL");
                    }

                    if (!inifile.KeyExists("Host", "MySQL"))
                    {
                        inifile.Write("Host", "127.0.0.1", "MySQL");
                    }
                    else
                    {
                        _MySQLHost = inifile.Read("Host", "MySQL");
                    }
                    
                    if (!inifile.KeyExists("Port", "MySQL"))
                    {
                        inifile.Write("User", "3306", "MySQL");
                    }
                    else
                    {
                        _MySQLPort = inifile.Read("Port", "MySQL");
                    }
                    
                    if (!inifile.KeyExists("DB", "MySQL"))
                    {
                        inifile.Write("DB", "trickemu", "MySQL");
                    }
                    else
                    {
                        _MySQLDB = inifile.Read("DB", "MySQL");
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
                    try
                    {
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
            try {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Config.channelPort));
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
                Console.WriteLine(Config.encoding.GetString(recBuf));
                handlePacket(recBuf, current);
            }
            else
            {
                return;
            }

            current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            return;
        }

        public static void handlePacket(byte[] bytes, Socket current)
        {
            int i = bytes.Length;

            byte[] dec = Encryption.decrypt(bytes);

            if (bytes[4] == 0xD0 && bytes[5] == 0x07)
            {
                // Request char selection

                System.Threading.Thread.Sleep(50);

                byte[] msg = new byte[] { 0x4F, 0x01, 0x9E, 0x18, 0xD2, 0x07, 0x00, 0x00, 0x01, 0x61, 0x48, 0x6E, 0xEA, 0x6A, 0xA0, 0xA0, 0xA0, 0xA0, 0xC8, 0xD0, 0xBD, 0x6A, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0x4A, 0x4F, 0x57, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0x61, 0xA0, 0x61, 0xA0, 0xE1, 0xD8, 0x61, 0x01, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0x4C, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0x4C, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0x53, 0xBE, 0x2D, 0x9A, 0x92, 0x6D, 0xF6, 0x61, 0x53, 0xBE, 0x2D, 0x9A, 0x92, 0x6D, 0xF6, 0x61, 0xB4, 0xE1, 0xA0, 0xA0, 0x61, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0x61, 0x48, 0x6E, 0xEA, 0x6A, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0, 0xA0 };

                current.Send(msg);
            }
            else if (bytes[4] == 0xDC && bytes[5] == 0x07)
            {
                // Char select?

                System.Threading.Thread.Sleep(50);

                byte[] msg = new byte[] { 0x2D, 0x00, 0x00, 0x00, 0xDE, 0x07, 0x00, 0x00, 0x00, 0xD8, 0x1D, 0xF6, 0x05, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x3C, 0x15, 0x00, 0x00, 0x31, 0x32, 0x37, 0x2E, 0x30, 0x2E, 0x30, 0x2E, 0x31, 0x00, 0xF6, 0x55, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x00, 0x00, 0x86, 0x15, 0x00, 0x00, 0x00 };

                current.Send(msg);

                System.Threading.Thread.Sleep(50);

                byte[] msg2 = new byte[] { 0x13, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0B, 0xE1, 0xF5, 0x05, 0x00, 0x00, 0x00, 0x00 };

                current.Send(msg2);
            }
            else
            {
                Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, Language.strings["UnhandledPacket"]);
            }

            return;
        }
    }
}
