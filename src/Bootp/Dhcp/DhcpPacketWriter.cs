namespace dhcp
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    public class DhcpPacketWriter : IDisposable
    {
        public MemoryStream _memoryStream;
        public BinaryWriter _binaryWriter;

        public DhcpPacketWriter()
        {
            _memoryStream = new MemoryStream(1024);
            _binaryWriter = new BinaryWriter(_memoryStream);
        }

        public void Dispose()
        {
            _memoryStream.Flush();

            _binaryWriter.Dispose();
            _binaryWriter = null;

            _memoryStream.Dispose();
            _memoryStream = null;
        }

        public Byte[] ToArray()
        {
            return _memoryStream.ToArray();
        }

        public void WriteByte(Byte value)
        {
            _binaryWriter.Write(value);
        }

        public void WriteUInt16(UInt16 value)
        {
            value = (UInt16)IPAddress.HostToNetworkOrder((short)value);
            _binaryWriter.Write(value);
        }

        public void WriteUInt32(UInt32 value)
        {
            value = (UInt32)IPAddress.HostToNetworkOrder((int)value);
            _binaryWriter.Write(value);
        }

        public void WriteString(String value, int fieldLength)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            var length = Math.Min(bytes.Length, fieldLength - 1);
            _binaryWriter.Write(bytes, 0, length);

            bytes = new Byte[fieldLength - length];
            WriteBytes(bytes);
        }

        public void WriteIpAddress(IPAddress value)
        {
            var bytes = value.GetAddressBytes();
            WriteBytes(bytes);
        }

        public void WriteBytes(Byte[] value)
        {
            _binaryWriter.Write(value);
        }
    }
}
