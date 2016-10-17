namespace dhcp
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    public class TftpPacketReader : IDisposable
    {
        private Byte[] _data;
        private int _pos;

        public TftpPacketReader(Byte[] data)
        {
            _data = data;
            _pos = 0;
        }

        public void Dispose()
        {
        }

        public Boolean EndOfStream()
        {
            return _pos >= _data.Length;
        }

        public UInt16 ReadUInt16()
        {
            var number = (UInt16)(_data[_pos] << 8);
            _pos++;
            number |= _data[_pos];
            _pos++;
            return number;
        }

        public String ReadString()
        {
            var i = _pos;
            while ((i < _data.Length) && (0 != _data[i]))
            {
                i++;
            }

            i -= _pos;
            var str = Encoding.ASCII.GetString(_data, _pos, i);
            _pos += i + 1;
            return str;
        }

        public Byte[] ReadToEnd()
        {
            var length = _data.Length - _pos;
            var array = new Byte[length];
            Array.Copy(_data, _pos, array, 0, length);
            return array;
        }
    }
}
