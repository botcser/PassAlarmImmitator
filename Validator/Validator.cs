using Extensions;
using IRAPROM.MyCore.Device;
using IRAPROM.MyCore.Model.WP;
using IRAPROM.MyCore.MyNetwork;
using PassAlarmSimulator.Device.Simulator;
using System.Diagnostics;

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

            //_networkServerMatreshka = new DeviceNetworkServer(IRAPROM.MyCore.Device.Matreshka.Constants.PortUDPDefault, 
            //    IRAPROM.MyCore.Device.Matreshka.Constants.PortUDPListenDefault,
            //    IRAPROM.MyCore.Device.Matreshka.Constants.PortTCPDefault,
            //    new IRAPROM.MyCore.Device.Matreshka.DatagramProto(), _cancellationTokenSource,
            //    $"{Directory.GetCurrentDirectory()}/MatreshkaSimulator");

            //DeviceMetalDetector.FamilyInfoVariants[0].PortUDPAdditional = (short)port;
            //DeviceMetalDetector.FamilyInfoVariants[0].PortUDPListenAdditional = (short)listenPort;

            //_networkServerMatreshkaXPROGOST = new DeviceNetworkServer(IRAPROM.MyCore.Device.Matreshka.XGOST.Constants.PortUDPDefault,
            //    IRAPROM.MyCore.Device.Matreshka.XGOST.Constants.PortUDPListenDefault,
            //    IRAPROM.MyCore.Device.Matreshka.XGOST.Constants.PortTCPDefault,
            //    new IRAPROM.MyCore.Device.Matreshka.XGOST.DatagramProto(), _cancellationTokenSource,
            //    $"{Directory.GetCurrentDirectory()}/XPROGOSTSimulator");

            //DeviceMetalDetector.FamilyInfoVariants[2].PortUDPAdditional = (short)port;
            //DeviceMetalDetector.FamilyInfoVariants[2].PortUDPListenAdditional = (short)listenPort;

            //_networkServerImpulse = new DeviceNetworkServer(IRAPROM.MyCore.Device.Impulse.Constants.PortUDPDefault,
            //    IRAPROM.MyCore.Device.Impulse.Constants.PortUDPListenDefault, 
            //    IRAPROM.MyCore.Device.Impulse.Constants.PortTCPDefault,
            //    new IRAPROM.MyCore.Device.Impulse.DatagramProto(), _cancellationTokenSource,
            //    $"{Directory.GetCurrentDirectory()}/ImpulseSimulator");

            //DeviceMetalDetector.FamilyInfoVariants[1].PortUDPAdditional = (short)port;
            //DeviceMetalDetector.FamilyInfoVariants[1].PortUDPListenAdditional = (short)listenPort;
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

                    await Validate();

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

            if (!StaticTests()) return;

            //if (!await DynamicTests()) return;
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


        private bool StaticTests()
        {
            _watchdog.Start();

            var success = FoundDevices.All(device => device.StaticTest());

            _watchdog.Stop();

            Console.WriteLine("Have all device's pass counters been reset? (press 'y' if yes)");

            if (Console.ReadLine() != "y")
            {
                success = false;
            }

            Console.WriteLine($"{(success ? "...Static Tests OK." : "...Static Tests FAIL!")} {_watchdog.Elapsed.TotalSeconds}s");
            
            Console.WriteLine($"\nStatic Tests done____________");

            return success;
        }

    }
}
