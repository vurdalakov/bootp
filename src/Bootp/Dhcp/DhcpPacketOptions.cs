namespace dhcp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;

    public class DhcpPacketOptions
    {
        private Dictionary<DhcpPacketOptionId, DhcpPacketOption> _options = new Dictionary<DhcpPacketOptionId, DhcpPacketOption>();

        public DhcpPacketOptions()
        {
        }

        public DhcpPacketOptionId[] GetIds()
        {
            return _options.Keys.ToArray();
        }

        public void Clear()
        {
            _options.Clear();
        }

        public Boolean Exists(DhcpPacketOptionId id)
        {
            return _options.ContainsKey(id);
        }

        public Boolean Remove(DhcpPacketOptionId id)
        {
            return _options.Remove(id);
        }

        #region Get

        public Byte GetByte(DhcpPacketOptionId id, Byte defaultValue = 0)
        {
            return _options.ContainsKey(id) ? _options[id].Data[0] : defaultValue;
        }

        public UInt16 GetUInt16(DhcpPacketOptionId id, UInt16 defaultValue = 0)
        {
            var number = _options.ContainsKey(id) ? BitConverter.ToUInt16(_options[id].Data, 0) : defaultValue;
            return (UInt16)IPAddress.NetworkToHostOrder((short)number);
        }

        public UInt32 GetUInt32(DhcpPacketOptionId id, UInt32 defaultValue = 0)
        {
            var number = _options.ContainsKey(id) ? BitConverter.ToUInt32(_options[id].Data, 0) : defaultValue;
            return (UInt32)IPAddress.NetworkToHostOrder((int)number);
        }

        public String GetString(DhcpPacketOptionId id, String defaultValue = null)
        {
            return _options.ContainsKey(id) ? Encoding.ASCII.GetString(_options[id].Data, 0, _options[id].Data.Length) : defaultValue;
        }

        public IPAddress GetIpAAddress(DhcpPacketOptionId id, IPAddress defaultValue = null)
        {
            return _options.ContainsKey(id) ? new IPAddress(_options[id].Data) : defaultValue;
        }

        public Byte[] GetBytes(DhcpPacketOptionId id)
        {
            return _options.ContainsKey(id) ? _options[id].Data : null;
        }

        #endregion

        #region Set

        public void SetByte(DhcpPacketOptionId id, Byte value)
        {
            SetBytes(id, new Byte[] { value });
        }

        public void SetUInt16(DhcpPacketOptionId id, UInt16 value)
        {
            value = (UInt16)IPAddress.HostToNetworkOrder((short)value);
            SetBytes(id, BitConverter.GetBytes(value));
        }

        public void SetUInt32(DhcpPacketOptionId id, UInt32 value)
        {
            value = (UInt32)IPAddress.HostToNetworkOrder((int)value);
            SetBytes(id, BitConverter.GetBytes(value));
        }

        public void SetString(DhcpPacketOptionId id, String value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            SetBytes(id, bytes);
        }

        public void SetIpAAddress(DhcpPacketOptionId id, IPAddress value)
        {
            var bytes = value.GetAddressBytes();
            SetBytes(id, bytes);
        }

        public void SetBytes(DhcpPacketOptionId id, Byte[] value)
        {
            var option = new DhcpPacketOption(id, value);
            _options[id] = option;
        }

        #endregion

        #region DhcpPacketOption

        public class DhcpPacketOption
        {
            public DhcpPacketOptionId Id { get; private set; }
            public Byte Length { get { return (Byte)Data.Length; } }
            public Byte[] Data { get; private set; }

            public DhcpPacketOption(DhcpPacketOptionId id, Byte[] data)
            {
                Id = id;
                Data = data;
            }
        }

        #endregion
    }
}
