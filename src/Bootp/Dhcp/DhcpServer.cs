namespace dhcp
{
    using System;
    using System.Net;

    public class DhcpServer : UdpServer
    {
        public delegate void DhcpPacketReceivedEventHandler(DhcpServer server, DhcpPacket packet);
        public event DhcpPacketReceivedEventHandler PacketReceived = delegate { };

        public String Uefi32FileName { get; set; }
        public String Uefi64FileName { get; set; }

        public DhcpServer(IPAddress address, int port) : base(address, port)
        {
        }

        protected override void ProcessRequest(IPEndPoint remoteEndPoint, Byte[] data, int dataLength)
        {
            DhcpPacket.TraceToFile(data);

            var dhcpPacket = new DhcpPacket(data, dataLength);
            Console.WriteLine("{0:X8}: *** DHCP packet type {1} received on port {2}", dhcpPacket.xid, dhcpPacket.MessageType, Port);

            var vendorClassIdentifier = dhcpPacket.VendorClassIdentifier;
            if (String.IsNullOrEmpty(vendorClassIdentifier) || !vendorClassIdentifier.StartsWith("PXEClient"))
            {
                Console.WriteLine("{0:X8}: Not a BOOTP request", dhcpPacket.xid);
                return;
            }

            PacketReceived(this, dhcpPacket);

            switch (dhcpPacket.MessageType)
            {
                case DhcpMessageType.DhcpDiscover:
                    ProcessDiscoverRequest(dhcpPacket);
                    break;
                case DhcpMessageType.DhcpRequest:
                    if (Port != 4011)
                    {
                        Console.WriteLine("{0:X8}: Not a BOOTP request", dhcpPacket.xid);
                        return;
                    }
                    ProcessRequestRequest(dhcpPacket);
                    break;
                default:
                    Console.WriteLine("{0:X8}: Message type not handled", dhcpPacket.xid);
                    break;
            }
        }

        private DhcpPacket CreateResponsePacket(DhcpPacket requestPacket)
        {
            Console.WriteLine("{0:X8}: MaximumDhcpMessageSize: {1}", requestPacket.xid, requestPacket.MaximumDhcpMessageSize);
            Console.WriteLine("{0:X8}: VendorClassIdentifier: '{1}'", requestPacket.xid, requestPacket.VendorClassIdentifier);
            Console.WriteLine("{0:X8}: ClientSystemArchitecture: {1}", requestPacket.xid, requestPacket.ClientSystemArchitecture);

            var responsePacket = new DhcpPacket(requestPacket);

            responsePacket.op = 2;
            responsePacket.sname = "Vurdalakov.PxeServer." + Address.ToString();

            responsePacket.Options.Clear();
            responsePacket.Options.SetByte(DhcpPacketOptionId.DhcpMessageType, 0);
            responsePacket.Options.SetIpAAddress(DhcpPacketOptionId.ServerIdentifier, Address);

            return responsePacket;
        }

        private void ProcessDiscoverRequest(DhcpPacket requestPacket)
        {
            var responsePacket = CreateResponsePacket(requestPacket);

            responsePacket.Options.SetString(DhcpPacketOptionId.VendorClassIdentifier, "PXEClient");

            SendPacket(DhcpMessageType.DhcpOffer, responsePacket, IPAddress.Broadcast, 68);
        }

        private void ProcessRequestRequest(DhcpPacket requestPacket)
        {
            var responsePacket = CreateResponsePacket(requestPacket);

            responsePacket.siaddr = Address;

            switch (requestPacket.ClientSystemArchitecture)
            {
                case DhcpClientSystemArchitecture.ia86Pc:       // legacy BIOS
                    Console.WriteLine("Unsupported client system architecture: {0}", requestPacket.ClientSystemArchitecture);
                    break;
                case DhcpClientSystemArchitecture.EfiIa32:      // EFI x86
                    responsePacket.file = Uefi32FileName;
                    break;
                case DhcpClientSystemArchitecture.EfiBc:        // EFI x64
                case DhcpClientSystemArchitecture.Efix8664:
                    responsePacket.file = Uefi64FileName;
                    break;
                default:
                    Console.WriteLine("Unsupported client system architecture: {0}", requestPacket.ClientSystemArchitecture);
                    break;
            }

            SendPacket(DhcpMessageType.DhcpAcknowledge, responsePacket, requestPacket.ciaddr, 4011);
        }

        private void SendPacket(DhcpMessageType dhcpMessageType, DhcpPacket responsePacket, IPAddress address, int port)
        {
            Console.WriteLine("{0:X8}: Sending packet type {1} to {2}:{3}", responsePacket.xid, dhcpMessageType, address, port);

            responsePacket.Options.SetByte(DhcpPacketOptionId.DhcpMessageType, (Byte)dhcpMessageType);

            var broadcast = address == IPAddress.Broadcast;
            responsePacket.flags = (UInt16)(broadcast ? 0x8000 : 0x0000);

            var bytes = responsePacket.ToArray();
            DhcpPacket.TraceToFile(bytes);

            var endPoint = new IPEndPoint(address, port);
            SendData(bytes, endPoint);

            Console.WriteLine("{0:X8}: Packet sent", responsePacket.xid);
        }
    }
}
