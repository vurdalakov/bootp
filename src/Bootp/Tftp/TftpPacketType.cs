namespace dhcp
{
    using System;

    public enum TftpPacketType : UInt16
    {
        ReadRequest = 1,
        WriteRequest = 2,
        Data = 3,
        Acknowledgment = 4,
        Error = 5,
        OptionAcknowledgment = 6
    }
}
