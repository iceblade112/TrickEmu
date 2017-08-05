using MySql.Data.MySqlClient;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TELib;

namespace TrickEmu
{
    class Program
    {
        private static Socket _serverSocket;
        private static readonly List<Socket> _clientSockets = new List<Socket>();
        private static readonly byte[] _buffer = new byte[2048];

        public static MySqlConnection _MySQLConn;
        public static Maps Maps;
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static int _entityIdx;
        public static Dictionary<int, Character> _clientPlayers = new Dictionary<int, Character>();
        public static string _GameIP = "127.0.0.1";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode; // UTF-16
            Console.Title = "TrickEmu Game (0.50)";

            var config = new Configuration();
            Maps = new Maps();
               
            // MySQL
            _MySQLConn = new MySqlConnection("server=" + config.DB["Host"] + ";port=3306;database=" + config.DB["Database"] + ";uid=" + config.DB["Username"] + ";pwd=" + config.DB["Password"] + ";");
            try
            {
                _MySQLConn.Open();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to connect to MySQL.");
                Console.ReadKey();
                Environment.Exit(1); // Exit with error code 1 because error
            }

            logger.Info("Starting server...");
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, int.Parse(config.Server["GamePort"])));
                _serverSocket.Listen(5);
                _serverSocket.BeginAccept(AcceptCallback, null);
                logger.Info("Server has been started on port {0}.", config.Server["GamePort"]);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unable to start the server.");
                Console.ReadKey();
                Environment.Exit(1); // Exit with error code 1 because error
            }

            new Timer(DisconnectTimer, null, 0, 1000);

            while (true) Console.ReadLine();
        }

        private static void DisconnectTimer(Object o)
        {
            List<int> disposedPlayers = new List<int>();

            foreach (KeyValuePair<int, Character> entry in Program._clientPlayers)
            {
                bool disposed = false;
                try
                {
                    if (entry.Value.Socket.Connected)
                    {
                        disposed = false;
                    }
                    else
                    {
                        // Fix char dupe bug
                        disposed = true;
                    }
                    if (entry.Value.ClientRemoved && !entry.Value.ChangingMap)
                    {
                        disposed = true;
                    }
                }
                catch (ObjectDisposedException)
                {
                    disposed = true;
                }

                if (disposed == true)
                {
                    disposedPlayers.Add(entry.Key);
                }
            }


            foreach (int key in disposedPlayers)
            {
                foreach (KeyValuePair<int, Character> plr in Program._clientPlayers)
                {
                    if (plr.Key == key)
                    {
                        continue;
                    }

                    try
                    {
                        // Disconnected
                        PacketBuffer dcmsg = new PacketBuffer();
                        dcmsg.WriteHeaderHexString("06 00 00 00 01");
                        dcmsg.WriteUshort(Program._clientPlayers[key].EntityID);

                        plr.Value.Socket.Send(dcmsg.getPacket());
                    }
                    catch
                    {
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
            socket.BeginReceive(_buffer, 0, 2048, SocketFlags.None, ReceiveCallback, socket);
            logger.Info("A client has been accepted from port {0}.", socket.RemoteEndPoint.ToString());
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
                if (!current.Connected || received == 0)
                {
                    GameFunctions.DisconnectPlayer(current);

                    logger.Info("{0} gracefully disconnected.", current.RemoteEndPoint.ToString());
                    _clientSockets.Remove(current);
                    current.Close();
                    return;
                }
            }
            catch (SocketException)
            {
                logger.Warn("Client {0} forcefully disconnected.", current.RemoteEndPoint.ToString());
                GameFunctions.DisconnectPlayer(current);
                current.Close();
                _clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);

            if (received != 0)
            {
                try
                {
                    Packets._PacketReader.HandlePacket(current, recBuf);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Unable to handle packet. Perhaps a malformed packet was sent?");
                    GameFunctions.DisconnectPlayer(current);
                }
            }
            else
            {
                return;
            }

            current.BeginReceive(_buffer, 0, 2048, SocketFlags.None, ReceiveCallback, current);
            return;
        }
    }
}
