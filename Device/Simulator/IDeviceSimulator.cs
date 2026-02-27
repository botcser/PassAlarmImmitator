using IRAPROM.MyCore.Device;

namespace PassAlarmSimulator.Device.Simulator
{
    public abstract class DeviceSimulator : IDisposable
    {
        private readonly DeviceNetworkServer _networkServer;
        private CancellationTokenSource _cancellationTokenSource;

        public DeviceSimulator(int inputUdpPort, int outputUdpPort, int tcpPort, string dirPathResponses, IDatagramProto datagramProto, bool oldPC = false)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _networkServer = new DeviceNetworkServer(inputUdpPort, outputUdpPort, tcpPort, datagramProto, _cancellationTokenSource, dirPathResponses, oldPC);
        }

        public Task Start()
        {
            var task = new Task(() =>
            {
                _networkServer.Run();
            });

            task.Start();

            return task;
        }

        public void Shutdown()
        {
            _cancellationTokenSource.Cancel();
            Dispose();
        }

        public void Dispose()
        {
            _networkServer?.Dispose();
        }
    }
}