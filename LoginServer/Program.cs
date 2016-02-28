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
            Console.OutputEncoding = Encoding.Unicode; // UTF-16
            Console.Title = "TrickEmu Login (0.50)";

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
                try
                {
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
                    }
                    else
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
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, Config.loginPort));
                _serverSocket.Listen(5);
                _serverSocket.BeginAccept(AcceptCallback, null);
                Methods.echoColor(Language.strings["SocketSys"], ConsoleColor.DarkGreen, Language.strings["StartedServer"], new string[] { Config.loginPort.ToString() });
            }
            catch (Exception ex)
            {
                Console.WriteLine(Language.strings["ErrorStartServer"] + ex);
            }
            while(true) Console.ReadLine();
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
                handlePacket(recBuf, current);
            } else
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

            if (bytes[4] == 0xED && bytes[5] == 0x2C)
            {
                // Login request

                // Invalid password (60001):
                //byte[] msg = new byte[] { 0x0D, 0x00, 0x00, 0x00, 0xEF, 0x2C, 0x00, 0x00, 0x00, 0x63, 0xEA, 0x00, 0x00 };

                byte[] msg = new byte[] { };
                
                string uid = Methods.getString(dec, i).Substring(0, 12);
                string upw = Methods.getString(dec, i).Substring(19);

                try {
                    using (MySqlCommand cmd = _MySQLConn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = @userid AND password = @userpw;";
                        cmd.Parameters.AddWithValue("@userid", Methods.cleanString(uid));
                        cmd.Parameters.AddWithValue("@userpw", Methods.cleanString(upw));
                        if (Convert.ToInt32(cmd.ExecuteScalar()) >= 1)
                        {
                            msg = new byte[] { 0x5F, 0x00, 0x00, 0x00, 0xEE, 0x2C, 0x00, 0x00, 0x00, 0x0B, 0xE1, 0xF5, 0x05, 0x65, 0x04, 0x60, 0x93, 0x3D, 0x8C, 0xF5, 0x0F, 0x01, 0x01, 0x01, 0x00, 0x53, 0x68, 0x61, 0x6E, 0x67, 0x68, 0x6F, 0x69, 0x00, 0xED, 0xCB, 0x01, 0x66, 0xC7, 0x53, 0x4E, 0x00, 0xD9, 0xC2, 0x00, 0xA8, 0xFB, 0xCB, 0x01, 0xAA, 0xDD, 0xC2, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0C, 0x57, 0x4F, 0x52, 0x4C, 0x44, 0x31, 0x00, 0x00, 0xD9, 0xC2, 0x00, 0xA8, 0xFB, 0xCB, 0x01, 0xAA, 0xDD, 0xC2, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAC, 0x0D, 0x00, 0x00 };
                            current.Send(msg);
                        } else
                        {
                            msg = new byte[] { 0x0D, 0x00, 0x00, 0x00, 0xEF, 0x2C, 0x00, 0x00, 0x00, 0x63, 0xEA, 0x00, 0x00 };
                            current.Send(msg);
                        }
                        cmd.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Database error: " + ex);
                }

                //Console.WriteLine("User ID: " + uid);
                //Console.WriteLine("User PW: " + upw);

                //Methods.echoColor("Packet Handler", ConsoleColor.Green, "Sent server select screen.");
            }
            else if (bytes[4] == 0xF1 && bytes[5] == 0x2C)
            {
                // Server log in (get IP)
                // 127.0.0.1
                byte[] msg = new byte[] { 0x1B, 0x00, 0x00, 0x00, 0xF2, 0x2C, 0x00, 0x00, 0x00, 0x31, 0x32, 0x37, 0x2E, 0x30, 0x2E, 0x30, 0x2E, 0x31, 0x00, 0x00, 0x00, 0x20, 0x01, 0x00, 0x00, 0x16, 0x27 };

                current.Send(msg);
            }
            else
            {
                Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, Language.strings["UnhandledPacket"]);
            }

            return;
        }
    }
}
