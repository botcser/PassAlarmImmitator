using System;
using Device;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PassAlarmSimulator.Device.Simulator
{
    public abstract class DeviceSimulator : IDisposable
    {
        private readonly DeviceNetworkServer _networkServer;
        private CancellationTokenSource _cancellationTokenSource;

        public DeviceSimulator(int inputUdpPort, int outputUdpPort, int tcpPort, string dirPathResponses, IDatagramProto datagramProto)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _networkServer = new DeviceNetworkServer(inputUdpPort, outputUdpPort, tcpPort, dirPathResponses, datagramProto, _cancellationTokenSource);
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
