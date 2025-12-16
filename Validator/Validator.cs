using Device.Matreshka;
using Extensions;
using PassAlarmSimulator.Device.Simulator;

namespace Validator
{
    public class Validator : IStart
    {
        private readonly DeviceNetworkServer _networkServerMatreshka;
        private readonly DeviceNetworkServer _networkServerImpulse;
        private CancellationTokenSource _cancellationTokenSource;

        public Validator()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _networkServerMatreshka = new DeviceNetworkServer(Constants.PortUDPDefault, Constants.PortUDPListenDefault, Constants.PortTCPDefault, new DatagramProto(), _cancellationTokenSource);
            _networkServerImpulse = new DeviceNetworkServer(Constants.PortUDPDefault, Constants.PortUDPListenDefault, Constants.PortTCPDefault, new DatagramProto(), _cancellationTokenSource);
        }

        public Task Start()
        {
            Console.WriteLine($"Validator: started");

            var task = new Task(() =>
            {
                var networkServer = new Task(() =>
                {
                    _networkServerMatreshka.Run();
                });

                networkServer.Start();
            });

            task.Start();

            return task;
        }

        public void Shutdown()
        {
            _networkServerMatreshka.Shutdown();
        }
    }
}
