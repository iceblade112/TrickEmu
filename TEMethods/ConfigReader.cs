using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu
{
    /// <summary>
    /// Fake INI reader.
    /// Does not follow actual INI rules.
    /// At least it works on Mono...
    /// </summary>
    public class ConfigReader
    {
        private string inipath = "TESettings.cfg";
        private Dictionary<string, string> vals = new Dictionary<string, string>();

        public ConfigReader(string path)
        {
            inipath = path;
            try {
                string cfgraw = File.ReadAllText(path);
                string[] cfgspl = cfgraw.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.RemoveEmptyEntries);
                foreach(string s in cfgspl)
                {
                    if(s.StartsWith("#") || s.StartsWith("[") || s.EndsWith("]")) { continue; }
                    if(!s.Contains("=")) { continue; }
                    
                    string[] val = s.Split('=');
                    if(val.Length != 2) { continue; }
                    vals[val[0].Trim()] = val[1].Trim();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Couldn't do something with the config: " + ex);
            }
        }

        public void Write(string key, string val)
        {
            vals[key] = val;
        }

        public string Read(string key)
        {
            return vals[key];
        }

        public bool KeyExists(string key)
        {
            if(vals.ContainsKey(key))
            {
                return true;
            }
            return false;
        }

        public void Save()
        {
            try { File.Delete(inipath); } catch { }
            string final = "";
            foreach (KeyValuePair<string, string> val in vals)
            {
                final += val.Key + "=" + val.Value + Environment.NewLine;
            }
            File.WriteAllText(inipath, final);
        }
    }
}
