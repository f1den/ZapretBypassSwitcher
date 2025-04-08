using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BypassSwitcher {
    internal class INIIO
    {
        private string iniPath { get; }

        public INIIO(string path)
        {
            this.iniPath = path;
        }

        public void WriteValue(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, this.iniPath);
        }

        public string ReadValue(string section, string key)
        {
            StringBuilder builder = new StringBuilder(255);
            int ini = GetPrivateProfileString(section, key, "", builder, 255, this.iniPath);
            return builder.ToString();
        }

        public bool HasKey(string section, string key)
        {
            string val = ReadValue(section, key);
            if (val == null)
                return false;
            if (val.Length == 0)
                return false;
            return true;
        }

        //DLL IMPORTS
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string name, string key, string val,
                                                         string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def,
                                                          StringBuilder retVal, int size,
                                                          string filePath);
    }
}
