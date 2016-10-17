namespace dhcp
{
    using System;
    using System.Net;

    public class BootpServer
    {
        public void Start(IPAddress localIpAddress, String tftpRootDirectory, String uefi32FileName, String uefi64FileName)
        {
            var server = new DhcpServer(localIpAddress, 67);
            server.Start();

            var proxy = new DhcpServer(localIpAddress, 4011);
            proxy.Uefi32FileName = uefi32FileName;
            proxy.Uefi64FileName = uefi64FileName;
            proxy.Start();

            var tftpServer = new TftpServer(localIpAddress, tftpRootDirectory);
            tftpServer.Start();
        }
    }
}
