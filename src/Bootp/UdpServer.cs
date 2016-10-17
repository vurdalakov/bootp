namespace dhcp
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public abstract class UdpServer : IDisposable
    {
        private Socket _socket;
        private readonly Byte[] _socketBuffer = new Byte[1024];

        private readonly ReaderWriterLock _readerWriterLock = new ReaderWriterLock();
        private Boolean _abort;

        public IPAddress Address { get; private set; }
        public int Port { get; private set; }

        public UdpServer(IPAddress address, int port)
        {
            Address = address;
            Port = port;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            // set socket option SIO_UDP_CONNRESET to "false" (ignore ICMP "port unreachable" messages when sending an UDP datagram)
            // https://msdn.microsoft.com/en-us/library/windows/desktop/bb736550.aspx
            // TODO: Windows XP only?
            const uint IOC_IN = 0x80000000;
            const uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            _socket.IOControl((int)SIO_UDP_CONNRESET, new[] { Convert.ToByte(false) }, null);
        }

        public void Dispose()
        {
            _readerWriterLock.AcquireWriterLock(-1);
            try
            {
                _abort = true;
                _socket = null;
            }
            finally
            {
                _readerWriterLock.ReleaseLock();
            }
        }

        public void Start()
        {
            try
            {
                var endPoint = new IPEndPoint(Address, Port);
                _socket.Bind(endPoint);
                Console.WriteLine("Server listening on {0}:{1}", Address, Port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not bind: {0}", ex.Message);
            }

            ReceiveData();
        }

        private void ReceiveData()
        {
            _readerWriterLock.AcquireReaderLock(-1);

            try
            {
                if (_abort)
                {
                    return;
                }

                EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                _socket.BeginReceiveFrom(_socketBuffer, 0, _socketBuffer.Length, SocketFlags.None, ref remote, new AsyncCallback(this.OnReceive), _socketBuffer);

            }
            finally
            {
                _readerWriterLock.ReleaseLock();
            }
        }

        private void OnReceive(IAsyncResult result)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.CompleteRequest), result);
            ReceiveData();
        }

        private void CompleteRequest(Object state)
        {
            try
            {
                IAsyncResult result = (IAsyncResult)state;

                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var dataLength = _socket.EndReceiveFrom(result, ref remoteEndPoint);

                ProcessRequest(remoteEndPoint as IPEndPoint, (Byte[])result.AsyncState, dataLength);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.Message);
#if DEBUG
                Console.WriteLine("Exception: {0}", ex.ToString());
#endif
                //throw ex; // TODO??
            }
        }

        protected abstract void ProcessRequest(IPEndPoint remoteEndPoint, Byte[] data, int dataLength);

        protected void SendData(Byte[] data, int length, IPEndPoint remoteEndPoint)
        {
            Console.WriteLine("Sending {0} bytes to {1}:{2}", length, remoteEndPoint.Address, remoteEndPoint.Port);
            var broadcast = remoteEndPoint.Address == IPAddress.Broadcast;
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, broadcast);

            _socket.SendTo(data, 0, length, SocketFlags.None, remoteEndPoint);
            Console.WriteLine("Packet sent");
        }

        protected void SendData(Byte[] data, IPEndPoint remoteEndPoint)
        {
            SendData(data, data.Length, remoteEndPoint);
        }
    }
}
