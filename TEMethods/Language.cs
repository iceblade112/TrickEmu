using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu
{
    public class Language
    {
        // English is default
        public static string lang = "en";

        // Defaults
        public static Dictionary<string, string> strings = new Dictionary<string, string>();

        public Language()
        {
            strings["PacketHandler"] = "Packet Handler";
            strings["SocketSys"] = "Socket System";
            strings["General"] = "General";
            strings["ConfigNotExist"] = "Configuration file does not exist. Creating.";
            strings["ConfigErrorUseDefault"] = "An error occurred while creating the default configuration file. Defaults will be used.";
            strings["MySQLConnectError"] = "An error occurred while connecting to MySQL. Press any key to exit.";
            strings["StartingServer"] = "Starting server...";
            strings["StartedServer"] = "Server has been started on port {0}.";
            strings["ErrorStartServer"] = "An error occurred while starting the server: ";
            strings["UnhandledPacket"] = "Unhandled packet received";
	        strings["ForcefulDisconnect"] = "Client {0} disconnected forcefully.";
            strings["GracefulDisconnect"] = "Client {0} disconnected.";
            strings["ClientAccepted"] = "Client accepted from {0}.";
        }

        public static void loadFromFile(string lang)
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\languages"))
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\languages\\" + lang + ".ini"))
                {
                    Console.WriteLine("Loading language...");
                    var inifile = new ConfigReader(AppDomain.CurrentDomain.BaseDirectory + "\\languages\\" + lang + ".ini");

                    foreach (string key in strings.Keys.ToArray())
                    {
                        if (inifile.KeyExists(key))
                        {
                            strings[key] = inifile.Read(key);
                        }
                    }
                }
            }
        }
    }
}
