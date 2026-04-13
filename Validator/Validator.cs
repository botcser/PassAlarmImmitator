using Extensions;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Model.WP;
using IRAPROM.MyCore.MyNetwork;
using PassAlarmSimulator.Device.Simulator;
using System;
using System.Diagnostics;
using System.Linq;

namespace PassAlarmSimulator.Validator
{
    public class Validator : IStart
    {
        public static List<DeviceMetalDetector> FoundDevices = new List<DeviceMetalDetector>();

        private readonly DeviceNetworkServer _networkServerMatreshka;
        private readonly DeviceNetworkServer _networkServerMatreshkaXPROGOST;
        private readonly DeviceNetworkServer _networkServerImpulse;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly string _ip;

        private Stopwatch _watchdog = new Stopwatch();

        public Validator(string ip, int port = 0, int listenPort = 0)
        {
            _ip = ip;
            _cancellationTokenSource = new CancellationTokenSource();

            if (port != 0)
            {
                DeviceMetalDetector.FamilyInfoVariants.ForEach(i => i.PortUDPAdditional = (short)port);
            }
        }

        public Task Start()
        {
            Console.WriteLine($"Validator: started");

            var task = new Task(async void () =>
            {
                try
                {
                    StartListeners();

                    FindDevices(_ip);

                    if (FoundDevices.Count > 0)
                    {
                        FixIpCollisions();

                        InitAllFoundDevices();

                        await Validate();
                    }

                    Console.WriteLine($"\nValidator: job is done. Press any key to exit.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\nValidator: EX: {e.Message}!");
                }
            });

            task.Start();

            return task;
        }

        private void FixIpCollisions()
        {
            var randomSubIPs = GenerateUniqueRandomNumbers(25, 4, 254);
            var randomIPs = randomSubIPs.Select(i => $"192.168.1.{i}").ToList();

            FoundDevices.ForEach(i => randomIPs.Remove(i.IP));

            do
            {
                var collisionsIps = FoundDevices.Select(i => i.IP).GroupBy(x => x).Where(group => group.Count() > 1).Select(group => group.Key).ToList();
                var collisionDevices = FoundDevices.Where(i => collisionsIps.Contains(i.IP));

                if (!collisionDevices.Any()) break;

                Console.WriteLine($"\nCollision IP are:");

                foreach (var ip in collisionsIps)
                {
                    Console.WriteLine($"\t{ip}");
                }

                if (randomIPs.Count < collisionsIps.Count)
                {
                    throw new Exception(
                        $"Validator: FixIpCollisions: randomIPs.Count{randomIPs.Count} !< collisionsIps.Count{collisionsIps.Count} IP collisions are too many than free IPs!");
                }

                FoundDevices.ForEach(i =>
                {
                    if (!collisionsIps.Contains(i.IP)) return;       

                    collisionsIps.Remove(i.IP);

                    var ip = randomIPs.FirstOrDefault();

                    randomIPs.Remove(ip);
                    
                    Console.WriteLine($"\nChanging IP from {i.IP} to {ip}");
                    i.SetIp(ip);
                });

                Console.WriteLine($"Waiting for device setup IP {19000}ms");
                Thread.Sleep(19000);
            } while (true);

            var device = FoundDevices.FirstOrDefault(i => i.IP == "192.168.1.3");

            if (device != null)
            {
                var ip = randomIPs.FirstOrDefault();

                Console.WriteLine($"\nChanging IP from {device.IP} to {ip} because this IP is reserved for test");

                device.SetIp(ip);

                Console.WriteLine($"Waiting for device setup IP {19000}ms");
                Thread.Sleep(19000);
            }
        }

        private void StartListeners()
        {
            new UdpListenerServer(IRAPROM.MyCore.Device.Impulse.Constants.PortUDPListenDefault, null)?.StartListening();
            new UdpListenerServer(IRAPROM.MyCore.Device.Matreshka.Constants.PortUDPListenDefault, null)?.StartListening();
            new UdpListenerServer(IRAPROM.MyCore.Device.Matreshka.XGOST.Constants.PortUDPListenDefault, null)?.StartListening();

            if (DeviceMetalDetector.FamilyInfoVariants[0].PortUDPListenAdditional != 0)
            {
                new UdpListenerServer(DeviceMetalDetector.FamilyInfoVariants[0].PortUDPListenAdditional, null)?.StartListening();
            }

            if (DeviceMetalDetector.FamilyInfoVariants[1].PortUDPListenAdditional != 0)
            {
                new UdpListenerServer(DeviceMetalDetector.FamilyInfoVariants[1].PortUDPListenAdditional, null)?.StartListening();
            }

            if (DeviceMetalDetector.FamilyInfoVariants[2].PortUDPListenAdditional != 0)
            {
                new UdpListenerServer(DeviceMetalDetector.FamilyInfoVariants[2].PortUDPListenAdditional, null)?.StartListening();
            }

            Thread.Sleep(1000);
        }

        public void Shutdown()
        {
            _networkServerMatreshka?.Shutdown();
            _networkServerMatreshkaXPROGOST?.Shutdown();
            _networkServerImpulse?.Shutdown();
        }

        private void FindDevices(string ip)
        {
            FoundDevices.Clear();
            //DeviceMetalDetector.FamilyInfoVariants[2].Find(_ip, UDPSender.Instance);
            DeviceMetalDetector.FamilyInfoVariants.ForEach(i =>
            {
                i.Find(_ip, UDPSender.Instance);
            });

            WaitForSeconds(4);

            PrintFoundDevices();
        }

        private void PrintFoundDevices()
        {
            if (FoundDevices.Count == 0)
            {
                Console.WriteLine("No suitable devices were found!");

                return;
            }

            Console.WriteLine($"\nFound {FoundDevices.Count} devices:");

            FoundDevices.ForEach(PrintInfo);
        }

        private void PrintInfo(DeviceMetalDetector device)
        {
            Console.WriteLine($"\tSeries: {device.SeriesName} \tModel: {device.ModelName} \tIP: {device.IP} \tMAC: {device.MAC}");
        }

        private void WaitForSeconds(int seconds)
        {
            foreach (var second in Enumerable.Range(1, seconds))
            {
                Thread.Sleep(1000);

                Console.WriteLine($"... wait for {second}");
            }
        }

        private async Task Validate()
        {
            if (FoundDevices.Count == 0) return;

            WaitForSeconds(3);

            //if (HaveUndocumentedFeatures()) return;
            
            if (!StaticTests()) return;

            if (!await DynamicTests()) return;
        }

        private bool HaveUndocumentedFeatures()
        {
            Console.WriteLine($"____Testing for Undocumented Features____");

            _watchdog.Start();

            var success = FoundDevices.FirstOrDefault().BruteTest();

            _watchdog.Stop();
            
            Console.WriteLine($"____Testing Undocumented Features {(success ? "OK." : "FAIL!")} {_watchdog.Elapsed.TotalSeconds}s____");

            return success;
        }

        private bool InitAllFoundDevices()
        {
            FoundDevices.All(device =>
            {
#if DEBUG
                Console.WriteLine($"___PreStaticTest: GetWorkParams {device.IP}:{device.MAC}... ");
#endif
                var deviceType = device.FamilyInfo.GetType();

                if (deviceType != typeof(IRAPROM.MyCore.Device.Matreshka.XGOST.Constants))
                {
                    Console.WriteLine("Enter device Model:");
                    
                    if (deviceType == typeof(IRAPROM.MyCore.Device.Impulse.Constants))
                    {
                        device.ModelId = (ushort)FindOutModelId(IRAPROM.MyCore.Device.Impulse.Constants.Models);
                    }
                    else
                    {
                        device.ModelId = (ushort)FindOutModelId(IRAPROM.MyCore.Device.Matreshka.Constants.Models);
                    }
                }

                var workParams = device.GetWorkParams();

                workParams.MAC = device.MAC;
                workParams.IP = device.IP;
                device.WorkParams = workParams;

#if DEBUG
                Console.WriteLine("...OK ");
#endif

                return workParams != null;
            });

            return false;
        }

        private short FindOutModelId(Dictionary<string, (short ModelId, List<short> AvailableZonesCount, string Name, List<int> GridCellDefinitions, int RealCoilsCount)> models)
        {
            var index = 0;
            var indexes = Enumerable.Range(0, models.Count).ToArray();
            var modelsList = new List<string>();

            foreach (var model in models)
            {
                if (model.Value.ModelId >= 0xFE) continue;
                Console.WriteLine($"{indexes[index++]} - {model.Key}");
                modelsList.Add(model.Key);
            }

            index = int.Parse(Console.ReadLine() ?? string.Empty);

            return models[modelsList[index]].ModelId;
        }

        private async Task<bool> DynamicTests()                             // assume to start after Static tests!
        {
            Console.WriteLine($"\n____________Starting Dynamic Tests...");

            var testTasks = FoundDevices.Select(x => x.DynamicTest(20000));
            //var testTasks = FoundDevices.FirstOrDefault()!.DynamicTest(20000);

            var results = await Task.WhenAll(testTasks);

            var success = results.All(result => result);

            Console.WriteLine($"{(success ? "...Dynamic Tests OK." : "...Dynamic Tests FAIL!")}");

            Console.WriteLine($"\nDynamic Tests done____________");

            return success;
        }


        private bool StaticTests()
        {
            _watchdog.Start();
            
            var success = FoundDevices.All(device => device.StaticTest());
            //var success = FoundDevices.FirstOrDefault()!.StaticTest();

            _watchdog.Stop();

            //Console.WriteLine("Have all device's pass counters been reset? (press 'y' if yes)");

            //if (Console.ReadLine() != "y")
            //{
            //    success = false;
            //}

            Console.WriteLine($"{(success ? "...Static Tests OK." : "...Static Tests FAIL!")} {_watchdog.Elapsed.TotalSeconds}s");

            return success;
        }

        public static IEnumerable<int> GenerateUniqueRandomNumbers(int count, int minValue, int maxValue)
        {
            if (maxValue - minValue + 1 < count)
            {
                throw new ArgumentException("Range is too small to generate the requested number of unique elements.");
            }

            var rnd = new Random();
            var uniqueNumbers = Enumerable.Range(minValue, maxValue - minValue + 1).OrderBy(x => rnd.Next()).Where(i => i % 2 == 0).Take(count);

            return uniqueNumbers;
        }

    }
}
