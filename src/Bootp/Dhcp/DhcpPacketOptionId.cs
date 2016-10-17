namespace dhcp
{
    using System;

    public enum DhcpPacketOptionId : Byte
    {
        Pad = 0,
        DhcpMessageType = 53,
        ServerIdentifier = 54,
        ParameterRequestList = 55,
        MaximumDhcpMessageSize = 57,
        VendorClassIdentifier = 60,
        ClientIdentifier = 61,
        ClientSystemArchitecture = 93,
        ClientNetworkDeviceInterface = 94,
        ClientIdentifierUuid = 97,
        End = 255
    }
}
