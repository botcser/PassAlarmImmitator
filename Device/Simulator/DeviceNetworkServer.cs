using IRAPROM.MyCore.Device;
using System;
using System.Collections.Generic;
using System.IO;
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
        private bool _oldPC;

        public DeviceNetworkServer(int inputUdpPort, int outputUdpPort, int tcpPort, IDatagramProto datagramProto, CancellationTokenSource cancellationTokenSource, string dirPath = null, bool oldPC = false)
        {
            _inputUdpPort = inputUdpPort;
            _outputUdpPort = outputUdpPort;
            _tcpPort = tcpPort;
            _datagramProto = datagramProto;
            _cancellationTokenSource = cancellationTokenSource;
            _oldPC = oldPC;

            _commandExtractor = new CommandExtractor(dirPath);
            _tcpServer = new TcpListener(IPAddress.Any, _tcpPort);
            _udpInputClient = new UdpClient(inputUdpPort);
            _udpInputClient.EnableBroadcast = true;
        }

        public bool Run()
        {
            try
            {
                _tcpListener = StartTcpListener(_oldPC);
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

                    await SendAnswer(request.Buffer, code);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task SendAnswer(byte[] request, short code, NetworkStream stream = null)
        {
            var bytesCommand = FindResponse(request, code);

            Console.WriteLine($"Response: {BitConverter.ToString(bytesCommand)}");

            if (bytesCommand == Array.Empty<byte>()) return;

            if (stream == null)
            {
                await UDPSend(bytesCommand);
            }
            else
            {
                if (code == 0x41 || code == 0x42 || code == 0xAE)                    //TODO
                {
                    await UDPSend(bytesCommand);
                }
                else
                {
                    await stream.WriteAsync(bytesCommand, 0, bytesCommand.Length, _cancellationTokenSource.Token);
                }
            }
        }

        private async Task StartTcpListener(bool oldPC = false)
        {
            _tcpServer.Start();

            Console.WriteLine($"TCP Listener started on port {_tcpPort}");
            try
            {
                while (true)
                {
                    var client = await _tcpServer.AcceptTcpClientAsync(_cancellationTokenSource.Token);
                    var buffer = new byte[1024];

                    Console.WriteLine("TCP Client connected!");

                    using (var stream = client.GetStream())
                    {
                        while (client != null && client.Client != null && client.Connected)
                        {
                            var requestLen = await stream.ReadAsync(buffer, 0, buffer.Length, _cancellationTokenSource.Token);

                            if (requestLen == 0) break;

                            var request = new byte[requestLen];

                            Array.Copy(buffer, 0, request, 0, requestLen);

                            var code = _datagramProto.GetCodeFromDatagram(request);

                            Console.WriteLine($"Received TCP request from {client.Client.RemoteEndPoint}: {BitConverter.ToString(request)}: code {code:X2}");

                            switch (code)
                            {
                                case 0x42:
                                    if (oldPC)
                                    {
                                        await SendAnswer(request, 0x41);
                                        await SendAnswer(request, 0x42);
                                        await SendAnswer(request, 0x41);
                                    }
                                    else
                                    {
                                        await SendAnswer(request, 0x41);
                                        await SendAnswer(request, 0x42);
                                    }
                                    break;
                            case 0xae:
                                    await SendAnswer(request, 0xae);
                                    break;

                                default:
                                    await SendAnswer(request, code, stream);
                                    break;
                            }
                        }
                    }

                    Console.WriteLine($"TCP Client closed!");

                    client.Close();
                    client.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private Task UDPSend(byte[] bytes)
        {
            _udpInputClient.Client.SendTimeout = TimeSpan.FromSeconds(500).Milliseconds;

            return Task.Run(() =>
            {
                var ip = "255.255.255.255";
                _udpInputClient.SendAsync(bytes, bytes.Length, new IPEndPoint(IPAddress.Parse(ip), _outputUdpPort));

                Console.WriteLine($"UDPSend: SendAsync: {bytes.Length} bytes send to {ip}:{_outputUdpPort}!");
            });
        }

        private byte[] FindResponse(byte[] request, short code)          // TODO request
        {
            return _commandExtractor.ExtractCommand(code);
        }

        public void Shutdown()
        {
            _cancellationTokenSource.Cancel();
            Dispose();
        }

        public void Dispose()
        {
            _udpInputClient?.Dispose();
            _tcpServer?.Stop();
        }
    }
}