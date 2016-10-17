namespace dhcp
{
    using System;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BOOTP Server {0} | https://github.com/vurdalakov/bootp", Helpers.GetApplicationVersion());
            Console.WriteLine();

            if (args.Length != 1)
            {
                Console.WriteLine("Usage: bootp.exe <path/filename.efi>");
                return;
            }

            var fileName = args[0];
            fileName = '.' == fileName[0] ? Helpers.GetPathRelativeToExecutableDirectory(fileName) : fileName;
            var tftpRootDirectory = Path.GetDirectoryName(fileName);
            fileName = '/' + Path.GetFileName(fileName);

            var localIpAddresses = Helpers.GetLocalIpAddresses();
            if (0 == localIpAddresses.Length)
            {
                Console.WriteLine("No active network interface found.");
                return;
            }

            var localIpAddress = localIpAddresses[0];

            Console.WriteLine("Local IP address:   {0}", localIpAddress);
            Console.WriteLine("Default gateway:    {0}", Helpers.GetGatewayAddresses(localIpAddress)[0]);
            Console.WriteLine("TFTP root folder:   {0}", tftpRootDirectory);
            Console.WriteLine("UEFI app file name: {0}", fileName);
            Console.WriteLine();

            Console.WriteLine("BOOTP server is starting.");
            var bootpServer = new BootpServer();
            bootpServer.Start(localIpAddress, tftpRootDirectory, fileName, fileName);
            Console.WriteLine("BOOTP server is running.");

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();Console.Write("\r");
        }
    }
}
