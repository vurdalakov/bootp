namespace dhcp
{
    using System;

    public class TftpPacket
    {
        public TftpPacketType Type { get; private set; }
        public String FileName { get; private set; }
        public TftpMode Mode { get; private set; }
        public UInt16 BlockNumber { get; private set; }
        public Byte[] Data { get; private set; }
        public TftpError ErrorCode { get; private set; }
        public String ErrorMessage { get; private set; }

        public TftpPacketOptions Options { get; private set; }

        public Int32 BlockSize { get; private set; }
        public Int32 Timeout { get; private set; }
        public Int32 TSize { get; private set; }
        public Boolean Multicast { get; private set; }
        public Int32 WindowSize { get; private set; }

        public TftpPacket(TftpPacketType type)
        {
            Options = new TftpPacketOptions();

            Type = type;
        }

        public TftpPacket(Byte[] data, int dataLength)
        {
            Options = new TftpPacketOptions();

            FromArray(data, dataLength);
        }

        public void FromArray(Byte[] data, int dataLength = -1)
        {
            if (dataLength < 0)
            {
                dataLength = data.Length;
            }

            if (dataLength < 4)
            {
                Console.WriteLine("Message is too short: {0} bytes", dataLength); // TODO: throw?
            }

            using (var reader = new TftpPacketReader(data))
            {
                Type = (TftpPacketType)reader.ReadUInt16();

                switch (Type)
                {
                    case TftpPacketType.ReadRequest:
                    case TftpPacketType.WriteRequest:
                        FileName = reader.ReadString();
                        var mode = reader.ReadString();
                        Mode = StringToMode(mode);

                        while (!reader.EndOfStream()) // RFC 2347
                        {
                            var name = reader.ReadString().ToLower();
                            if (String.IsNullOrEmpty(name))
                            {
                                break;
                            }
                            var value = reader.ReadString();
                            Options.Add(name, value);
                        }

                        BlockSize = Options.GetNumber("blksize"); // RFC 2348
                        Timeout = Options.GetNumber("timeout"); // RFC 2349
                        TSize = Options.GetNumber("tsize"); // RFC 2349
                        Multicast = Options.Exists("multicast"); // RFC 2090
                        WindowSize = Options.GetNumber("windowsize"); // RFC 7440
                        break;
                    case TftpPacketType.Data:
                        BlockNumber = reader.ReadUInt16();
                        Data = reader.ReadToEnd();
                        break;
                    case TftpPacketType.Acknowledgment:
                        BlockNumber = reader.ReadUInt16();
                        break;
                    case TftpPacketType.Error:
                        ErrorCode = (TftpError)reader.ReadUInt16();
                        ErrorMessage = reader.ReadString();
                        break;
                    case TftpPacketType.OptionAcknowledgment:
                        break;
                    default:
                        Console.WriteLine("Unsupported packet type: {0}", (int)Type);
                        return;
                }
            }
        }

        public Byte[] ToArray()
        {
            using (var writer = new TftpPacketWriter(Type == TftpPacketType.Data ? Data.Length + 4: 1024))
            {
                writer.WriteUInt16((UInt16)Type);

                switch (Type)
                {
                    case TftpPacketType.ReadRequest:
                        break;
                    case TftpPacketType.WriteRequest:
                        break;
                    case TftpPacketType.Data:
                        break;
                    case TftpPacketType.Acknowledgment:
                        break;
                    case TftpPacketType.Error:
                        break;
                    case TftpPacketType.OptionAcknowledgment:
                        var names = Options.GetNames();
                        foreach (var name in names)
                        {
                            writer.WriteString(name);
                            var value = Options.GetString(name);
                            writer.WriteString(value);
                        }
                        break;
                    default:
                        Console.WriteLine("Unsupported packet type: {0}", (int)Type);
                        break;
                }

                return writer.ToArray();
            }
        }

        private TftpMode StringToMode(String mode)
        {
            if (mode.Equals("netascii", StringComparison.OrdinalIgnoreCase))
            {
                return TftpMode.Netascii;
            }
            else if (mode.Equals("octet", StringComparison.OrdinalIgnoreCase))
            {
                return TftpMode.Octet;
            }
            else if (mode.Equals("mail", StringComparison.OrdinalIgnoreCase))
            {
                return TftpMode.Mail;
            }
            else if (mode.Equals("binary", StringComparison.OrdinalIgnoreCase))
            {
                return TftpMode.Binary;
            }
            else
            {
                return TftpMode.Unknown;
            }
        }
    }
}
