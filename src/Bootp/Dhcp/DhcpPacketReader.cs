namespace dhcp
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    public class DhcpPacketReader : IDisposable
    {
        private MemoryStream _memoryStream;
        private BinaryReader _binaryReader;

        public DhcpPacketReader(Byte[] data)
        {
            _memoryStream = new MemoryStream(data, false);
            _binaryReader = new BinaryReader(_memoryStream);
        }

        public void Dispose()
        {
            _memoryStream.Flush();

            _binaryReader.Dispose();
            _binaryReader = null;

            _memoryStream.Dispose();
            _memoryStream = null;
        }

        public Byte ReadByte()
        {
            return _binaryReader.ReadByte();
        }

        public UInt16 ReadUInt16()
        {
            var number = _binaryReader.ReadUInt16();
            return (UInt16)IPAddress.NetworkToHostOrder((short)number);
        }

        public UInt32 ReadUInt32()
        {
            var number = _binaryReader.ReadUInt32();
            return (UInt32)IPAddress.NetworkToHostOrder((int)number);
        }

        public String ReadString(int fieldLength)
        {
            var bytes = ReadBytes(fieldLength);

            int length = 0;
            while ((length < fieldLength) && (bytes[length] > 0))
            {
                length++;
            }

            return 0 == length ? "" : Encoding.ASCII.GetString(bytes, 0, length);
        }

        public IPAddress ReadIpAddress()
        {
            var number = _binaryReader.ReadBytes(4);
            return new IPAddress(number);
        }

        public Byte[] ReadBytes(int count)
        {
            return _binaryReader.ReadBytes(count);
        }
    }
}
