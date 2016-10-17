namespace dhcp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;

    public static class Helpers
    {
        public static IPAddress[] GetLocalIpAddresses(Boolean getIp6Adresses = false)
        {
            var localIpAddresses = new List<IPAddress>();

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            if (networkInterfaces.Length > 0)
            {
                foreach (var networkInterface in networkInterfaces)
                {
                    if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }

                    var properties = networkInterface.GetIPProperties();

                    if (0 == properties.GatewayAddresses.Count)
                    {
                        continue;
                    }

                    foreach (var address in properties.UnicastAddresses)
                    {
                        var ipAddress = address.Address;
                        if ((!getIp6Adresses && (AddressFamily.InterNetwork == ipAddress.AddressFamily)) || (getIp6Adresses && (AddressFamily.InterNetworkV6 == ipAddress.AddressFamily)))
                        {
                            localIpAddresses.Add(ipAddress);
                        }
                    }
                }
            }

            return localIpAddresses.ToArray();
        }

        public static IPAddress[] GetGatewayAddresses(IPAddress localIpAddress)
        {
            var gatewayAddresses = new List<IPAddress>();

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            if (networkInterfaces.Length > 0)
            {
                foreach (var networkInterface in networkInterfaces)
                {
                    var properties = networkInterface.GetIPProperties();

                    foreach (var address in properties.UnicastAddresses)
                    {
                        if (address.Address.Equals(localIpAddress))
                        {
                            foreach (var gatewayAddress in properties.GatewayAddresses)
                            {
                                gatewayAddresses.Add(gatewayAddress.Address);
                            }
                        }
                    }
                }
            }

            return gatewayAddresses.ToArray();
        }

        public static String GetExecutableDirectory()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static String GetPathRelativeToExecutableDirectory(String relativeDirectory)
        {
            return Path.GetFullPath(Path.Combine(Helpers.GetExecutableDirectory(), relativeDirectory));
        }

        public static String GetApplicationVersion()
        {
            var parts = Assembly.GetExecutingAssembly().FullName.Split(',');
            return parts[1].Split('=')[1];
        }
    }
}
