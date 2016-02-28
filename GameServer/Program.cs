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

        // GameServer specifics
        private static int sitstate = 0; // TO-DO: Player class

        static void Main(string[] args)
        {
            new Language(); // Initialize default language strings
            Console.Title = "TrickEmu Game (0.50)";

            if (!File.Exists("TESettings.ini"))
            {
                Methods.echoColor(Language.strings["General"], ConsoleColor.DarkCyan, Language.strings["ConfigNotExist"]);
                try
                {
                    var inifile = File.Create("TESettings.ini");
                    inifile.Close();
                    var ini = new ConfigReader("TESettings.ini");
                    ini.Write("Language", "en", Language.strings["General"]);
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

            if (bytes[0] == 0x17 && bytes[1] == 0x00)
            {
                System.Threading.Thread.Sleep(500);

                byte[] msg = new byte[] { 0x13, 0x00, 0xC7, 0xFB, 0xE6, 0x00, 0x00, 0x00, 0x01, 0x42, 0xF6, 0x4D, 0x71, 0x93, 0x49, 0x45, 0xCA, 0x38, 0x30, 0x4E, 0x00, 0x35, 0x0F, 0xE7, 0x00, 0x00, 0x00, 0x01, 0xAB, 0x0F, 0x75, 0x81, 0x10, 0x9E, 0x4F, 0x1C, 0x3D, 0x0F, 0x0F, 0x0F, 0x0F, 0xB5, 0x0F, 0x75, 0x15, 0xC4, 0xBB, 0x4F, 0x1C, 0x3D, 0x0F, 0x0F, 0x0F, 0x0F, 0xB5, 0x0F, 0x1F, 0x44, 0xE1, 0xA9, 0x4F, 0x1C, 0x3D, 0x0F, 0x0F, 0x0F, 0x0F, 0x75, 0x0F, 0x1F, 0x5E, 0x24, 0xC6, 0x4F, 0x1C, 0x3D, 0x0F, 0x0F, 0x0F, 0x0F, 0x75, 0x0F, 0x52, 0x2A, 0x3B, 0x09, 0x4F, 0x1C, 0x3D, 0x0F, 0x0F, 0x0F, 0x0F, 0x75, 0x0F, 0x40, 0x1F, 0x0C, 0x01, 0xE7, 0x11, 0xE9, 0x00, 0x00, 0x00, 0x01, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0xFF, 0xC6, 0x8B, 0x5C, 0x2A, 0x2A, 0x2A, 0x2A, 0x77, 0xC6, 0x8B, 0x5C, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x71, 0x2A, 0x2A, 0x2A, 0x07, 0x2A, 0x45, 0xCA, 0x2A, 0x2A, 0x38, 0x30, 0x2A, 0x2A, 0x07, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x72, 0x2A, 0x63, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0xAA, 0xF1, 0x2B, 0x52, 0x0D, 0xBA, 0xC6, 0x71, 0xAA, 0xF1, 0x2B, 0x52, 0x0D, 0xBA, 0xC6, 0x71, 0x33, 0x30, 0x2A, 0x2A, 0x71, 0x0D, 0x00, 0xB7, 0xDB, 0xEA, 0x00, 0x00, 0x00, 0x01, 0x2A, 0x2A, 0x2A, 0x2A, 0x0D, 0x00, 0xD4, 0x5D, 0xEB, 0x00, 0x00, 0x00, 0x01, 0x9A, 0x9A, 0x9A, 0x9A, 0x0B, 0x00, 0x94, 0x52, 0x28, 0x01, 0x00, 0x00, 0x01, 0x9A, 0x9A, 0x0B, 0x00, 0x87, 0xD3, 0x29, 0x01, 0x00, 0x00, 0x01, 0x2A, 0x2A, 0x0B, 0x00, 0xE4, 0x77, 0x0F, 0x01, 0x00, 0x00, 0x01, 0x9A, 0x9A, 0x0F, 0x00, 0x02, 0x35, 0x10, 0x01, 0x00, 0x00, 0x01, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x09, 0x00, 0x02, 0x35, 0x05, 0x00, 0x00, 0x00, 0x00 };

                current.Send(msg);
            }
            else if ((bytes[4] == 0x36 /* maybe 0E? */) && bytes[1] == 0x00)
            {
                // Chat
                // Packet data starts at 9
                // substring 9 (10)

                System.Threading.Thread.Sleep(1);

                string chatString = Methods.sep(Methods.getString(dec, 0), "\x00");

                var chatdata = new List<byte>();
                var chatheader = new List<byte>();

                if (chatString.StartsWith("!gmc "))
                {
                    // GM chat

                    ushort cPacketId = 0x13C;
                    chatheader.AddRange(BitConverter.GetBytes(cPacketId));
                    chatheader.AddRange(new byte[] { /*0x39, 0x00,*/ 0x00, 0x00, 0x01 });

                    chatdata.AddRange(new byte[] { 0x00, 0x00 });
                    chatdata.AddRange(Config.encoding.GetBytes(Methods.sep(Methods.getString(dec, 0), "\x00").Substring(5)));
                    chatdata.AddRange(new byte[] { 0x00, 0x00 });
                }
                else
                {
                    // Normal chat

                    chatheader.AddRange(new byte[] { 0x39, 0x00, 0x00, 0x00, 0x01 });

                    chatdata.AddRange(new byte[] { 0x94, 0x1B, 0x42, 0x6F, 0x62, 0x00 });
                    chatdata.AddRange(Config.encoding.GetBytes(Methods.sep(Methods.getString(dec, i), "\x00")));
                    chatdata.Add(0x00);
                }



                byte[] newpkt = Encryption.encrypt(chatheader.ToArray(), chatdata.ToArray());

                foreach(Socket sock in _clientSockets)
                {
                    try
                    {
                        if(sock != current)
                        {
                            sock.Send(newpkt);
                        }
                    } catch { }
                }

                current.Send(newpkt);

                Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, "Received chat packet: " + chatString);
            }
            else if (bytes[0] == 0x09 && bytes[1] == 0x00)
            {
                byte[] msg = new byte[] { 0x2F, 0x00, 0x9C, 0xF3, 0x07, 0x00, 0x00, 0x00, 0x01, 0x9B, 0x84, 0x5B, 0xD0, 0x5B, 0xD0, 0x5B, 0xB7, 0xD0, 0x95, 0x9E, 0x99, 0x5A, 0x95, 0x9E, 0x99, 0x5A, 0x6C, 0x2E, 0x53, 0xD0, 0xD0, 0xD0, 0x5B, 0xD0, 0xD0, 0xD0, 0xD0, 0xD0, 0xD8, 0xD0, 0xD0, 0xD0, 0xE7, 0x45, 0xD6, 0x84, 0x5B, 0x09, 0x00, 0x9C, 0xF3, 0x35, 0x01, 0x00, 0x00, 0x00 };

                current.Send(msg);

                // 2?

                byte[] msg2 = new byte[] { 0x0B, 0x00, 0x52, 0x36, 0x24, 0x00, 0x00, 0x00, 0x01, 0x80, 0x03, 0x0B, 0x00, 0x41, 0x47, 0x25, 0x00, 0x00, 0x00, 0x01, 0xBF, 0xCD, 0x2C, 0x00, 0x9E, 0xAB, 0x08, 0x00, 0x00, 0x00, 0x01, 0x4A, 0xA0, 0xD8, 0xA0, 0xDC, 0x01, 0xA0, 0xA0, 0xA0, 0xC6, 0x6A, 0x3F, 0x3C, 0xC6, 0x6A, 0x3F, 0x3C, 0x23, 0x26, 0x54, 0xF5, 0x6C, 0x99, 0x84, 0xCE, 0x54, 0x36, 0xFF, 0x55, 0xA0, 0xA0, 0x05, 0xA0, 0x6A, 0xA0, 0x2E, 0x00, 0xF5, 0x22, 0x08, 0x00, 0x00, 0x00, 0x01, 0x28, 0x0F, 0x1F, 0x0F, 0xD6, 0x91, 0x0F, 0x0F, 0x0F, 0xED, 0xAB, 0x46, 0x42, 0xED, 0xAB, 0x46, 0x42, 0xA3, 0x9A, 0x94, 0xD3, 0x2D, 0x76, 0xA3, 0xF1, 0x72, 0x68, 0xD8, 0xEE, 0x7C, 0xB6, 0x0F, 0x0F, 0xBA, 0x0F, 0x64, 0x0F };

                current.Send(msg2);

                // 3 ...?

                byte[] msg3 = new byte[] { 0x0C, 0x00, 0x7C, 0x33, 0x42, 0x00, 0x00, 0x00, 0x01, 0x6C, 0xD0, 0x8E };

                current.Send(msg3);
            }
            // ??????????
            /*else if (bytes[4] == 0x18 && bytes[5] == 0x00)
            {
                // Movement packet
                byte[] msg = new byte[] { 0x14, 0x00, 0x25, 0xBE, 0x18, 0x00, 0x00, 0x00, 0x01, 0x18, 0x98, 0x29, 0x91, 0x97, 0x42, 0xCF, 0x91, 0x16, 0x42, 0x75 };

                stream.Write(msg, 0, msg.Length);

                echoColor(Language.strings["PacketHandler"], ConsoleColor.Green);
                Console.WriteLine("Moving packet sent " + bytes[0] + " " + bytes[1]);
            }*/
            else if (bytes[4] == 0x40 && bytes[5] == 0x00)
            {
                byte[] msg = new byte[] { };

                // Header (SEND and RECV):
                // 40 00 00 00 01

                // Sit SEND (initial standing) byte is   02
                // Sit RECV (initial standing) bytes are 98 1B 02
                
                var sitsend = new List<byte>();

                sitsend.AddRange(new byte[] { 0x98, 0x1B, 0x02 });
                
                byte[] newpkt = Encryption.encrypt(new byte[] { 0x40, 0x00, 0x00, 0x00, 0x01 }, sitsend.ToArray());
                
                current.Send(newpkt);

                sitstate++;

                Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, "Sit packet sent. State " + sitstate);
            }
            else
            {
                Methods.echoColor(Language.strings["PacketHandler"], ConsoleColor.Green, Language.strings["UnhandledPacket"]);
            }

            return;
        }
    }
}
