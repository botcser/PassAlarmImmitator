using Extensions;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Model.WP;
using IRAPROM.MyCore.MyNetwork;
using PassAlarmSimulator.Device.Simulator;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace PassAlarmSimulator.Validator
{
    public class Validator : IStart
    {
        [DllImport("Iphlpapi.dll", SetLastError = true)]
        public static extern int FlushIpNetTable(int dwIfIndex);

        public static List<DeviceMetalDetector> FoundDevices = new List<DeviceMetalDetector>();

        private readonly DeviceNetworkServer _networkServerMatreshka;
        private readonly DeviceNetworkServer _networkServerMatreshkaXPROGOST;
        private readonly DeviceNetworkServer _networkServerImpulse;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly string _ip;
        private readonly bool _alarm, _clean;
        private short _portUdpAdditional;
        private short _portUdpListen;

        private Stopwatch _watchdog = new Stopwatch();

        public Validator(string ip, int port = 0, int listenPort = 0, bool alarm = false, bool clean = false)
        {
            _ip = ip;
            _portUdpAdditional = (short)port;
            _portUdpListen = (short)listenPort;
            _alarm = alarm;
            _clean = clean;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task Start()
        {
            Console.WriteLine($"Validator: started");

            await Task.Run(async Task () =>
            {
                try
                {
                    StartListeners();

                    FindDevices(_ip);

                    FixIpCollisions();

                    FlushAllArp(14);

                    InitAllFoundDevices();

                    if (FoundDevices.Count == 0)
                    {
                        Console.WriteLine($"\nValidator: job is done. Press any key to exit.");
                        Console.ReadLine();
                        Environment.Exit(0);
                    }

                    if (_alarm || _clean)
                    {
                        await TestPassages(_alarm, _clean);
                    }
                    else
                    {
                        await Validate();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"\nValidator: EX: {e.Message}!");
                }
            });

            return;
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
                    i.SetIp(ip, i.Mask);
                });

                Console.WriteLine($"Waiting for device setup IP {19000}ms");
                Thread.Sleep(19000);
            } while (true);

            var device = FoundDevices.FirstOrDefault(i => i.IP == "192.168.1.3");

            if (device != null)
            {
                var ip = randomIPs.FirstOrDefault();

                Console.WriteLine($"\nChanging IP from {device.IP} to {ip} because this IP is reserved for test");

                device.SetIp(ip, device.Mask);

                Console.WriteLine($"Waiting for device setup IP {19000}ms");
                Thread.Sleep(19000);
            }
        }

        private void StartListeners()
        {
            new UdpListenerServer(IRAPROM.MyCore.Device.Impulse.Constants.PortUDPListenDefault, null)?.StartListening();
            new UdpListenerServer(IRAPROM.MyCore.Device.Matreshka.Constants.PortUDPListenDefault, null)?.StartListening();
            new UdpListenerServer(IRAPROM.MyCore.Device.Matreshka.XGOST.Constants.PortUDPListenDefault, null)?.StartListening();

            if (_portUdpListen != 0)
            {
                new UdpListenerServer(_portUdpListen, null)?.StartListening();      // TODO
            }

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
            Console.WriteLine($"\tSeries: {device.SeriesName} \tModel: {device.ModelName}({device.ModelId:X}) \tIP: {device.IP} \tMAC: {device.MAC}");
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
            return FoundDevices.All(device =>
            {
#if DEBUG
                Console.WriteLine($"\t\t___PreStaticTest: GetWorkParams {device.IP}:{device.MAC}:{device.ModelId:X}... ");
#endif
                var deviceType = device.FamilyInfo.GetType();

                if (deviceType != typeof(IRAPROM.MyCore.Device.Matreshka.XGOST.Constants))
                {
                    Console.WriteLine("Enter device Model:");
                    
                    if (deviceType == typeof(IRAPROM.MyCore.Device.Impulse.Constants))
                    {
                        device.ModelId = (ushort)AskModelId(IRAPROM.MyCore.Device.DeviceMetalDetector.FamilyInfoVariants[1].Models);
                    }
                    else
                    {
                        device.ModelId = (ushort)AskModelId(IRAPROM.MyCore.Device.DeviceMetalDetector.FamilyInfoVariants[0].Models);
                    }
                }

                var workParams = device.GetWorkParams();

                workParams.MAC = device.MAC;
                workParams.IP = device.IP;
                device.WorkParams = workParams;

#if DEBUGG
                Console.WriteLine("...OK ");
#endif

                return workParams != null;
            });
        }

        private ushort AskModelId(Dictionary<ushort, MetalDetectorAttrs> models)
        {
            var index = 0;
            var indexes = Enumerable.Range(0, models.Count).ToArray();
            var modelsList = new List<string>();

            foreach (var model in models)
            {
                if (model.Key >= 0xFE) continue;

                Console.WriteLine($"{indexes[index++]} - {model.Key}");

                modelsList.Add(model.Value.ModelName);
            }

            index = int.Parse(Console.ReadLine() ?? string.Empty);

            return models.FirstOrDefault(i => i.Value.ModelName == modelsList[index]).Key;
        }

        private async Task<bool> DynamicTests()                             // assume to start after Static tests!
        {
            Console.WriteLine($"\n____________Starting Dynamic Tests...");

            var testTasks = FoundDevices.Select(x => x.DynamicTest(20000));

            var results = await Task.WhenAll(testTasks);

            var success = results.All(result => result);

            Console.WriteLine($"{(success ? "...Dynamic Tests OK." : "...Dynamic Tests FAIL!")}");

            Console.WriteLine($"\nDynamic Tests done____________");

            return success;
        }

        private async Task<bool> TestPassages(bool alarm, bool clean)
        {
            Console.WriteLine($"\n____________Starting test Passages...");

            uint alarmCount = 0, alarmCountNew = 0;
            uint cleanCount = 0, cleanCountNew = 0;
            uint deltaAlarmClean = 0, deltaAlarmCleanNew = 0;

            FoundDevices.ForEach(i =>
            {
                i.CleanStatistics();
            });

            if (alarm)
            {
                if (clean)
                {
                    Console.WriteLine($"\n\tMake sequential passes Clean-Alarm-Clean-Alarm...");
                }
                else
                {
                    Console.WriteLine($"\n\tMake sequential passes Alarm-Alarm-Alarm-Alarm...");
                }
            }
            else
            {
                Console.WriteLine($"\n\tMake sequential passes Clean-Clean-Clean-Clean...");
            }

            do
            {
                await Task.Delay(200);

                FoundDevices.ForEach(i =>
                {
                    alarmCountNew += i.LastPassage.EnterAlarmCount + i.LastPassage.ExitAlarmCount - alarmCount;
                    cleanCountNew += i.LastPassage.EnterPassagesCount + i.LastPassage.ExitPassagesCount - cleanCount;
                }); 
                deltaAlarmCleanNew = Math.Max(alarmCountNew, cleanCountNew) - Math.Min(alarmCountNew, cleanCountNew);

                if (alarmCountNew - alarmCount > 1 || cleanCountNew - cleanCount > 1)
                {
                    Console.WriteLine($"\nTestPassages: Fail: it was more then one Alarm/Clean!");
                    return false;
                }

                if (alarmCountNew != alarmCount)
                {
                    Console.Write($"_Alarm_");
                }
                if (clean && !alarm)
                {
                    if (alarmCountNew != alarmCount)
                    {
                        Console.Write($"_FAIL_!!!");
                        return false;
                    }
                }
                if (alarmCountNew != alarmCount)
                {
                    alarmCount = alarmCountNew;
                }

                if (cleanCountNew != cleanCount)
                {
                    Console.Write($"_Clean_");
                }
                if (alarm && !clean)
                {
                    if (cleanCountNew != cleanCount)
                    {
                        Console.Write($"_FAIL_!!!");
                        return false;
                    }
                }
                if (cleanCountNew != cleanCount)
                {
                    cleanCount = cleanCountNew;
                }

                if (alarm && clean)
                {
                    if (deltaAlarmCleanNew - deltaAlarmClean > 1)
                    {
                        Console.Write($"_FAIL_!!!");
                        return false;
                    }
                }

            } while (!_cancellationTokenSource.IsCancellationRequested);

            Console.WriteLine($"\n____________Passages test done.");
            return true;
        }

        private bool StaticTests()
        {
            _watchdog.Start();
            
            var success = FoundDevices.All(device =>
            {
                FlushAllArp(14);

                return device.StaticTest();
            });

            _watchdog.Stop();

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

        private void FlushAllArp(int index = 0)
        {
            Console.WriteLine($"Flushing ARP cache {index}");

            var result = FlushIpNetTable(index);

            if (result != 0)
            {
                Console.WriteLine($"FlushAllArp: Failed with error code: {result}");
            }
        }
    }
}
