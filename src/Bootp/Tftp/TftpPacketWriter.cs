namespace dhcp
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    public class TftpPacketWriter : IDisposable
    {
        public MemoryStream _memoryStream;
        public BinaryWriter _binaryWriter;

        public TftpPacketWriter(int capacity)
        {
            _memoryStream = new MemoryStream(capacity);
            _binaryWriter = new BinaryWriter(_memoryStream);
        }
        public TftpPacketWriter(Byte[] buffer)
        {
            _memoryStream = new MemoryStream(buffer, true);
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
            WriteByte((Byte)((value >> 8) & 0xFF));
            WriteByte((Byte)(value & 0xFF));
        }

        public void WriteString(String value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            WriteBytes(bytes);

            WriteByte(0);
        }

        public void WriteString(UInt32 value)
        {
            WriteString(value.ToString());
        }

        public void WriteBytes(Byte[] value)
        {
            _binaryWriter.Write(value);
        }
    }
}
