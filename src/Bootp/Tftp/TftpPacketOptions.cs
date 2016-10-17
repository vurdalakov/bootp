namespace dhcp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TftpPacketOptions
    {
        private Dictionary<String, String> _options = new Dictionary<String, String>();

        public String[] GetNames()
        {
            return _options.Keys.ToArray();
        }

        public Boolean Exists(String name)
        {
            return _options.ContainsKey(name.ToLower());
        }

        public void Add(String name, String value)
        {
            _options[name.ToLower()] = value;
        }

        public void Add(String name, Int32 value)
        {
            Add(name, value.ToString());
        }

        public String GetString(String name, String defaultValue = null)
        {
            name = name.ToLower();
            return _options.ContainsKey(name) ? _options[name] : defaultValue;
        }

        public Int32 GetNumber(String name, Int32 defaultValue = -1)
        {
            var octets = GetString(name);
            if (null == octets)
            {
                return defaultValue;
            }

            Int32 value = 0;
            return Int32.TryParse(octets, out value) ? value : defaultValue;
        }
    }
}
