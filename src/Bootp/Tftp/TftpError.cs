namespace dhcp
{
    using System;

    public enum TftpError
    {
        NotDefined = 0,
        FileNotFound = 1,
        AccessViolation = 2,
        DiskFull = 3,
        IllegalOperation = 4,
        UnknownTransferId = 5,
        FileAlreadyExists = 6,
        NoSuchUser = 7,
        OptionNegotiationFailed = 8
    }
}
