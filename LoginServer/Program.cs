using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TrickEmu
{
    class Program
    {
        private static Socket _serverSocket;
        private static readonly List<Socket> _clientSockets = new List<Socket>();
        private const int _BUFFER_SIZE = 2048;
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];
        private static int _PORT = 14446;

        static void Main(string[] args)
        {
            Console.Title = "TrickEmu Login (0.50)";
            Methods.echoColor("Socket System", ConsoleColor.DarkGreen, "Starting server...");
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _PORT));
                _serverSocket.Listen(5);
                _serverSocket.BeginAccept(AcceptCallback, null);
                Methods.echoColor("Socket System", ConsoleColor.DarkGreen, "Server has been started on port {0}.", new string[] { _PORT.ToString() });
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while starting the server: " + ex);
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
            Methods.echoColor("Socket System", ConsoleColor.DarkGreen, "Client accepted from " + socket.RemoteEndPoint + ".");
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
                Methods.echoColor("Socket System", ConsoleColor.DarkGreen, "Client " + current.RemoteEndPoint + " disconnected forcefully.");
                current.Close();
                _clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);

            if (received != 0)
            {
                Console.WriteLine("OK");
                handlePacket(recBuf, current);
            } else
            {
                return;
            }
            
            Console.WriteLine("BeginReceive 1");
            current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            Console.WriteLine("BeginReceive 2");
            
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
                
                byte[] msg = new byte[] { 0x5F, 0x00, 0x00, 0x00, 0xEE, 0x2C, 0x00, 0x00, 0x00, 0x0B, 0xE1, 0xF5, 0x05, 0x65, 0x04, 0x60, 0x93, 0x3D, 0x8C, 0xF5, 0x0F, 0x01, 0x01, 0x01, 0x00, 0x53, 0x68, 0x61, 0x6E, 0x67, 0x68, 0x6F, 0x69, 0x00, 0xED, 0xCB, 0x01, 0x66, 0xC7, 0x53, 0x4E, 0x00, 0xD9, 0xC2, 0x00, 0xA8, 0xFB, 0xCB, 0x01, 0xAA, 0xDD, 0xC2, 0x00, 0x01, 0x00, 0x00, 0x00, 0x0C, 0x57, 0x4F, 0x52, 0x4C, 0x44, 0x31, 0x00, 0x00, 0xD9, 0xC2, 0x00, 0xA8, 0xFB, 0xCB, 0x01, 0xAA, 0xDD, 0xC2, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAC, 0x0D, 0x00, 0x00 };

                Methods.echoColor("Packet Handler", ConsoleColor.Green, "Received login packet.");

                string uid = Methods.getString(dec, i).Substring(0, 12);
                string upw = Methods.getString(dec, i).Substring(19);

                Console.WriteLine("User ID: " + uid);
                Console.WriteLine("User PW: " + upw);

                current.Send(msg);

                Methods.echoColor("Packet Handler", ConsoleColor.Green, "Sent server select screen.");
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
                Methods.echoColor("Packet Handler", ConsoleColor.Green, "Unhandled packet received");
            }

            return;
        }
    }
}
