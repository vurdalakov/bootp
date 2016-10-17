namespace dhcp
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;

    public class TftpServer : UdpServer
    {
        private String _rootDirectory;

        public TftpServer(IPAddress address, String rootDirectory) : base(address, 69)
        {
            _rootDirectory = rootDirectory;
        }

        protected override void ProcessRequest(IPEndPoint remoteEndPoint, Byte[] data, int dataLength)
        {
            var requestPacket = new TftpPacket(data, dataLength);
            Console.WriteLine("*** TFTP packet type {0} received on port {1}", requestPacket.Type, Port);

            switch (requestPacket.Type)
            {
                case TftpPacketType.ReadRequest:
                    Console.WriteLine("FileName: '{0}'", requestPacket.FileName);
                    Console.WriteLine("Mode: {0}", requestPacket.Mode);
                    Console.WriteLine("BlockSize: {0}", requestPacket.BlockSize);
                    Console.WriteLine("Timeout: {0}", requestPacket.Timeout);
                    Console.WriteLine("TSize: {0}", requestPacket.TSize);
                    Console.WriteLine("WindowSize: {0}", requestPacket.WindowSize);
                    Console.WriteLine("Multicast: {0}", requestPacket.Multicast);
                    ProcessReadRequest(remoteEndPoint, requestPacket);
                    break;
                case TftpPacketType.WriteRequest:
                    Console.WriteLine("FileName: '{0}'", requestPacket.FileName);
                    Console.WriteLine("Mode: {0}", requestPacket.Mode);
                    Console.WriteLine("BlockSize: {0}", requestPacket.BlockSize);
                    Console.WriteLine("Timeout: {0}", requestPacket.Timeout);
                    Console.WriteLine("TSize: {0}", requestPacket.TSize);
                    Console.WriteLine("WindowSize: {0}", requestPacket.WindowSize);
                    SendNotSupportedError(remoteEndPoint);
                    break;
                case TftpPacketType.Data:
                    Console.WriteLine("BlockNumber: {0}", requestPacket.BlockNumber);
                    Console.WriteLine("Data: {0} bytes", requestPacket.Data.Length);
                    SendNotSupportedError(remoteEndPoint);
                    break;
                case TftpPacketType.Acknowledgment:
                    Console.WriteLine("BlockNumber: {0}", requestPacket.BlockNumber);
                    ProcessAcknowledgment(remoteEndPoint, requestPacket);
                    break;
                case TftpPacketType.Error:
                    Console.WriteLine("ErrorCode: {0}", requestPacket.ErrorCode);
                    Console.WriteLine("ErrorMessage: '{0}'", requestPacket.ErrorMessage);
                    break;
                case TftpPacketType.OptionAcknowledgment:
                    SendNotSupportedError(remoteEndPoint);
                    break;
                default:
                    SendNotSupportedError(remoteEndPoint);
                    return;
            }
        }

        FileInfo _fileInfo;
        Int32 _blockSize;
        Byte[] _fileBlock;
        Int32 _timeout;

        private void ProcessReadRequest(IPEndPoint remoteEndPoint, TftpPacket requestPacket)
        {
            if (String.IsNullOrEmpty(requestPacket.FileName))
            {
                return; // TODO
            }

            if (requestPacket.Mode != TftpMode.Octet)
            {
                SendError(TftpError.IllegalOperation, "Transfer mode not supported", remoteEndPoint);
                return;
            }

            var fileName = requestPacket.FileName.Replace('/', '\\');
            if ('\\' == fileName[0])
            {
                fileName = fileName.Substring(1);
            }
            fileName = Path.Combine(_rootDirectory, fileName);
            Console.WriteLine("Local file name: '{0}'", fileName);

            _fileInfo = new FileInfo(fileName);

            if (!_fileInfo.Exists)
            {
                SendError(TftpError.FileNotFound, "File not found", remoteEndPoint);
                return;
            }

            var responsePacket = new TftpPacket(TftpPacketType.OptionAcknowledgment);

            if (0 == requestPacket.TSize)
            {
                responsePacket.Options.Add("tsize", (Int32)_fileInfo.Length);
            }

            _blockSize = 512;
            if (requestPacket.BlockSize > 0)
            {
                _blockSize = requestPacket.BlockSize;
                responsePacket.Options.Add("blksize", _blockSize);
            }

            _fileBlock = new byte[_blockSize + 4];

            _timeout = -1; // TODO: implement
            if (requestPacket.Timeout > 0)
            {
                _timeout = requestPacket.Timeout;
                responsePacket.Options.Add("timeout", _timeout);
            }

            SendData(responsePacket.ToArray(), remoteEndPoint);
        }

        private void ProcessAcknowledgment(IPEndPoint remoteEndPoint, TftpPacket requestPacket)
        {
            var position = requestPacket.BlockNumber * _blockSize;
            if (position >= _fileInfo.Length)
            {
                Console.WriteLine("File transfer successfully finished");
                return;
            }

            var length = Math.Min(_blockSize, (Int32)_fileInfo.Length - position);

            using (var writer = new TftpPacketWriter(_fileBlock))
            {
                writer.WriteUInt16((UInt16)TftpPacketType.Data);
                writer.WriteUInt16((UInt16)(requestPacket.BlockNumber + 1));

                using (var fileStream = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    fileStream.Position = position;
                    fileStream.Read(_fileBlock, 4, (Int32)length);
                }

                SendData(_fileBlock, length + 4, remoteEndPoint);
            }
        }

        private void SendNotSupportedError(IPEndPoint remoteEndPoint)
        {
            const String errorMessage = "Packet type not supported";
            Console.WriteLine(errorMessage);
            SendError(TftpError.IllegalOperation, errorMessage, remoteEndPoint);
        }

        private void SendError(TftpError errorCode, String errorMessage, IPEndPoint remoteEndPoint)
        {
            using (var writer = new TftpPacketWriter(errorMessage.Length + 5))
            {
                writer.WriteUInt16((UInt16)TftpPacketType.Error);
                writer.WriteUInt16((UInt16)errorCode);
                writer.WriteString(errorMessage);

                SendData(writer.ToArray(), remoteEndPoint);
            }
        }
    }
}
