namespace dhcp
{
    using System;

    public enum DhcpMessageType : Byte
    {
        DhcpDiscover = 1,
        DhcpOffer = 2,
        DhcpRequest = 3,
        DhcpDecline = 4,
        DhcpAcknowledge = 5,
        DhcpNak = 6,
        DhcpRelease = 7,
        DhcpInform = 8
    }
}
