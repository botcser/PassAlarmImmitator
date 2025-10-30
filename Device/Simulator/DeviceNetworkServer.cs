using Assets.Common;
using Device;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable All

namespace PassAlarmSimulator.Device.Simulator
{
    public class DeviceNetworkServer : IDisposable
    {
        private static List<IPAddress> _localAddresses;
        
        private readonly UdpClient _udpInputClient;
        private readonly TcpListener _tcpServer;
        private readonly int _inputUdpPort;
        private readonly int _outputUdpPort;
        private readonly int _tcpPort;
        private readonly CommandExtractor _commandExtractor;
        private readonly IDatagramProto _datagramProto;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private Task _tcpListener;
        private Task _udpListener;
        
        public DeviceNetworkServer(int inputUdpPort, int outputUdpPort, int tcpPort, string dirPath, IDatagramProto datagramProto, CancellationTokenSource cancellationTokenSource)
        {
            _inputUdpPort = inputUdpPort;
            _outputUdpPort = outputUdpPort;
            _tcpPort = tcpPort;
            _datagramProto = datagramProto;
            _cancellationTokenSource = cancellationTokenSource;

            _commandExtractor = new CommandExtractor(dirPath);
            _tcpServer = new TcpListener(IPAddress.Any, _tcpPort);
            _udpInputClient = new UdpClient(inputUdpPort);
        }

        public bool Run()
        {
            try
            {
                _tcpListener = StartTcpListener();
                _udpListener = StartUdpListener();

                Task.WaitAny(_tcpListener, _udpListener);

                _cancellationTokenSource.Cancel();
            }
            catch (Exception e)
            {
                Console.WriteLine($"EX: DeviceNetworkServer: Run: {e.Message}!!!");

                return false;
            }

            return true; 
        }

        private async Task StartUdpListener()
        {
            Console.WriteLine($"UDP Listener started on port {_inputUdpPort}");

            try
            {
                while (true)
                {
                    var request = await _udpInputClient.ReceiveAsync(_cancellationTokenSource.Token);
                    var code = _datagramProto.GetCodeFromDatagram(request.Buffer);

                    Console.WriteLine($"Received UDP request from {request.RemoteEndPoint}: {BitConverter.ToString(request.Buffer)}: code {code:X2}");

                    var bytesCommand = FindResponse(request.Buffer, code);

                    if (bytesCommand == Array.Empty<byte>()) continue;

                    _udpInputClient.Client.SendTimeout = TimeSpan.FromSeconds(5).Milliseconds;

                    await _udpInputClient.SendAsync(bytesCommand, bytesCommand.Length, new IPEndPoint(IPAddress.Parse("255.255.255.255"), _outputUdpPort));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task StartTcpListener()
        {
            _tcpServer.Start();

            Console.WriteLine($"TCP Listener started on port {_tcpPort}");
            try
            {
                while (true)
                {
                    var client = await _tcpServer.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                    var buffer = new byte[1024];

                    using (var stream = client.GetStream())
                    {
                        while (client != null && client.Client != null && client.Connected)
                        {
                            Console.WriteLine("TCP Client connected!");

                            var requestLen = await stream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);
                            var request = new byte[requestLen];

                            Array.Copy(buffer, 0, request, 0, requestLen);

                            var code = _datagramProto.GetCodeFromDatagram(request);

                            Console.WriteLine($"Received TCP request from {client.Client.RemoteEndPoint}: {BitConverter.ToString(request)}: code {code:X2}");

                            var bytesCommand = FindResponse(buffer, code);

                            if (bytesCommand == Array.Empty<byte>()) continue;

                            await stream.WriteAsync(bytesCommand, 0, bytesCommand.Length, _cancellationTokenSource.Token);
                        }
                    }

                    client.Close();
                    client.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private byte[] FindResponse(byte[] request, byte code)
        {
            return _commandExtractor.ExtractCommand(code);
        }

        public void Dispose()
        {
            _udpInputClient?.Dispose();
            _tcpServer?.Stop();
        }
    }
}