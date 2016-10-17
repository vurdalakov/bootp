namespace dhcp
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    public class DhcpPacket
    {
        // RFC 951
        public Byte op { get; set; }
        public Byte htype { get; set; }
        public Byte hlen { get; set; }
        public Byte hops { get; set; }
        public UInt32 xid { get; set; }
        public UInt16 secs { get; set; }
        public UInt16 flags { get; set; }
        public IPAddress ciaddr { get; set; }
        public IPAddress yiaddr { get; set; }
        public IPAddress siaddr { get; set; }
        public IPAddress giaddr { get; set; }
        public Byte[] chaddr { get; set; }
        public String sname { get; set; }
        public String file { get; set; }

        // RFC 1497
        public DhcpPacketOptions Options { get; private set; }

        public DhcpMessageType MessageType { get { return (DhcpMessageType)Options.GetByte(DhcpPacketOptionId.DhcpMessageType); } }
        public UInt16 MaximumDhcpMessageSize { get { return Options.GetUInt16(DhcpPacketOptionId.MaximumDhcpMessageSize); } }
        public String VendorClassIdentifier { get { return Options.GetString(DhcpPacketOptionId.VendorClassIdentifier); } }
        public DhcpClientSystemArchitecture ClientSystemArchitecture { get { return (DhcpClientSystemArchitecture)Options.GetUInt16(DhcpPacketOptionId.ClientSystemArchitecture); } }

        private const UInt32 DhcpMessageMagicCookie = 0x63825363;

        public DhcpPacket(Byte[] data, int dataLength) // for request packet
        {
            FromArray(data, dataLength);
        }

        public DhcpPacket(DhcpPacket dhcpPacket) // for response packet
        {
            FromArray(dhcpPacket.ToArray());
        }

        public void FromArray(Byte[] data, int dataLength = -1)
        {
            if (dataLength < 0)
            {
                dataLength = data.Length;
            }

            if (dataLength < 241)
            {
                Console.WriteLine("Message is too short: {0} bytes", dataLength); // TODO: throw?
            }

            Options = new DhcpPacketOptions();

            using (var reader = new DhcpPacketReader(data))
            {

                op = reader.ReadByte();
                htype = reader.ReadByte();
                hlen = reader.ReadByte();
                hops = reader.ReadByte();
                xid = reader.ReadUInt32();
                secs = reader.ReadUInt16();
                flags = reader.ReadUInt16();
                ciaddr = reader.ReadIpAddress();
                yiaddr = reader.ReadIpAddress();
                siaddr = reader.ReadIpAddress();
                giaddr = reader.ReadIpAddress();
                chaddr = reader.ReadBytes(16);
                sname = reader.ReadString(64);
                file = reader.ReadString(128);

                var magicCookie = reader.ReadUInt32();
                if (magicCookie != DhcpMessageMagicCookie)
                {
                    Console.WriteLine("Wrong magic cookie: 0x{0:X8} instead of 0x{1:X8}", magicCookie, DhcpMessageMagicCookie); // TODO: throw?
                }

                while (true)
                {
                    var id = (DhcpPacketOptionId)reader.ReadByte();

                    if (DhcpPacketOptionId.Pad == id)
                    {
                        continue;
                    }
                    else if (DhcpPacketOptionId.End == id)
                    {
                        break;
                    }

                    var length = reader.ReadByte();

                    Options.SetBytes(id, reader.ReadBytes(length));
                }
            }
        }

        public Byte[] ToArray()
        {
            using (var writer = new DhcpPacketWriter())
            {

                writer.WriteByte(op);
                writer.WriteByte(htype);
                writer.WriteByte(hlen);
                writer.WriteByte(hops);
                writer.WriteUInt32(xid);
                writer.WriteUInt16(secs);
                writer.WriteUInt16(flags);
                writer.WriteIpAddress(ciaddr);
                writer.WriteIpAddress(yiaddr);
                writer.WriteIpAddress(siaddr);
                writer.WriteIpAddress(giaddr);
                writer.WriteBytes(chaddr);
                writer.WriteString(sname, 64);
                writer.WriteString(file, 128);

                writer.WriteUInt32(DhcpMessageMagicCookie);

                foreach (var id in Options.GetIds())
                {
                    writer.WriteByte((Byte)id);

                    var bytes = Options.GetBytes(id);

                    writer.WriteByte((Byte)bytes.Length);
                    writer.WriteBytes(bytes);
                }

                writer.WriteByte((Byte)DhcpPacketOptionId.End);

                return writer.ToArray();
            }
        }

        #region TraceToFile

        private static String TraceBinary(Byte[] data)
        {
            var stringBuilder = new StringBuilder(data.Length * 3 + 4);
            stringBuilder.AppendFormat("{0}:", data.Length);
            foreach (var b in data)
            {
                stringBuilder.AppendFormat("{0},", b);
            }
            return stringBuilder.ToString();
        }

        public static void TraceToFile(Byte[] data, String fileName = null)
        {
#if false // use Wireshark instead
            if (String.IsNullOrEmpty(fileName))
            {
                var directoryName = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

                var op = data.Length > 0 ? data[0] : 0;

                fileName = String.Format("pkt_{0}_{1}.txt", DateTime.Now.Ticks, 1 == op ? "in" : (2 == op ? "out" : "unknown"));
                fileName = Path.Combine(directoryName, fileName);
            }

            using (var streamWriter = new StreamWriter(fileName, false))
            {
                using (var reader = new DhcpPacketReader(data))
                {
                    streamWriter.WriteLine("--- packet.length={0}", data.Length);

                    streamWriter.WriteLine("op={0}", reader.ReadByte());
                    streamWriter.WriteLine("htype={0}", reader.ReadByte());
                    streamWriter.WriteLine("hlen={0}", reader.ReadByte());
                    streamWriter.WriteLine("hops={0}", reader.ReadByte());
                    streamWriter.WriteLine("xid={0}", reader.ReadUInt32());
                    streamWriter.WriteLine("secs={0}", reader.ReadUInt16());
                    streamWriter.WriteLine("flags={0}", reader.ReadUInt16());
                    streamWriter.WriteLine("ciaddr={0}", reader.ReadIpAddress());
                    streamWriter.WriteLine("yiaddr={0}", reader.ReadIpAddress());
                    streamWriter.WriteLine("siaddr={0}", reader.ReadIpAddress());
                    streamWriter.WriteLine("giaddr={0}", reader.ReadIpAddress());
                    streamWriter.WriteLine("chaddr={0}", TraceBinary(reader.ReadBytes(16)));
                    streamWriter.WriteLine("sname={0}", reader.ReadString(64));
                    streamWriter.WriteLine("file={0}", reader.ReadString(128));
                    streamWriter.WriteLine("mcookie=0x{0:X8}", reader.ReadUInt32());

                    while (true)
                    {
                        var id = reader.ReadByte();

                        if ((Byte)DhcpPacketOptionId.Pad == id)
                        {
                            streamWriter.WriteLine("ID {0}", id);
                            continue;
                        }
                        else if ((Byte)DhcpPacketOptionId.End == id)
                        {
                            streamWriter.WriteLine("ID {0}", id);
                            break;
                        }

                        var length = reader.ReadByte();
                        streamWriter.WriteLine("ID {0} {1}", id, TraceBinary(reader.ReadBytes(length)));
                    }
                }
            }
#endif
        }

        #endregion
    }
}
