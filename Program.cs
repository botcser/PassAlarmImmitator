
using Extensions;
using IRAPROM.MyCore.Device;
using PassAlarmSimulator.Validator;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

await App.Main();

public class App
{
    public static int Loader_UDPPortRetransmission = 0; // заглушка импорта

    public static async Task Main()
    {
        Console.WriteLine("Hello, World! Current PID: {Environment.ProcessId}");
        Console.WriteLine("Choose the program, press number:\n\t 1 = PC Pass Alarm Simulator \n\t 2 = PC Validator \n\t 0 = Exit\n");

        IStart task = null;

        var programNumber = Console.ReadLine();                                                                       // <== User Input

        switch (programNumber)
        {
            case "1":
                task = new PassAlarmSimulator.PassAlarmSimulator();
                break;
            case "2":
                var ip = "192.168.1.255";                                                                       // <== User Input
                //var ip = InitIP();
                Console.WriteLine($"Testing in {ip} network..."); 
                
                Console.WriteLine($"Choose the program, press number:\n\t 1 = Auto Tests \n\t 2 = Passage validate \n");
                programNumber = Console.ReadLine();                                                                   // <== User Input

                switch (programNumber)
                {
                    case "1":
                        //Console.WriteLine($"Enter lower computer UDP port\n");
                        //int.TryParse(Console.ReadLine(), out var port);

                        //Console.WriteLine($"Enter higher computer UDP port to listen\n");
                        //int.TryParse(Console.ReadLine(), out var portListen); \n\t 3 = PC 

                        task = new Validator(ip, 0, 0);
                        break;
                    case "2":
                        Console.WriteLine($"Choose the program, press number:\n\t 1 = Clean only \n\t 2 = Alarm only \n\t 3 = Clean_Alarm both\n");
                        programNumber = Console.ReadLine();                                                                // <== User Input

                        switch (programNumber)
                        {
                            case "1":
                                task = new Validator(ip, clean: true);
                                break;
                            case "2":
                                task = new Validator(ip, alarm: true);
                                break;
                            case "3":
                                task = new Validator(ip, clean: true, alarm: true);
                                break;
                            default:
                                break;
                        }
                        break;
                }
                break;
            case "0":
                Console.WriteLine("Goodbye, World!");
                Environment.Exit(0);
                break;
        }

        task?.Start().Wait();

        Console.WriteLine($"\nValidator: job is done. Press any key to exit.");
        Console.ReadLine();


        string InitIP()
        {
            var ips = new List<string>();

            foreach (var ip in InitNetworksIp())
            {
                ips.Add(ip);
            }

            Console.WriteLine($"Choose the ip subnetwork:\n");

            foreach (var ip in ips)
            {
                Console.WriteLine($"\t{ips.IndexOf(ip)} = {ip}");
            }
            
            return ips[int.Parse(Console.ReadLine() ?? string.Empty)];
        }

        IEnumerable<string> InitNetworksIp()
        {
            var NetworksIps = new List<string>();

            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.OperationalStatus != OperationalStatus.Up ||
                    adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var properties = adapter.GetIPProperties();

                foreach (var ip in properties.UnicastAddresses)
                {
                    switch (ip.Address.AddressFamily)
                    {
                        case AddressFamily.InterNetwork:
                            NetworksIps.Add(GetBroadcastAddress(ip.Address, IPAddress.Parse("255.255.255.0")));
                            break;
                    }
                }
            }

            NetworksIps.TryAdd("192.168.16.255");
            NetworksIps.Sort();
            NetworksIps.TryAdd("FF02::1"); // IPv6 "broadcast"

            var defaultIpIndex = NetworksIps.FirstOrDefault(i => i.Contains("192.168.16."));

            if (defaultIpIndex.IsNullOrEmpty()) return NetworksIps;

            NetworksIps.Remove(defaultIpIndex);
            NetworksIps.Insert(0, defaultIpIndex);

            return NetworksIps;


            string GetBroadcastAddress(IPAddress address, IPAddress mask)
            {
                var ipAddress = BitConverter.ToUInt32(address.GetAddressBytes(), 0);
                var ipMaskV4 = BitConverter.ToUInt32(mask.GetAddressBytes(), 0);
                var broadCastIpAddress = ipAddress | ~ipMaskV4;

                return new IPAddress(BitConverter.GetBytes(broadCastIpAddress)).ToString();
            }
        }
    }
}
