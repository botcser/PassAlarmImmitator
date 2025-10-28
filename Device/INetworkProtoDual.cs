using System.Net;
using System.Net.Sockets;
using Extensions;

namespace Device
{
    public interface INetworkProtoDual : INetworkProto
    {
        byte[] SendAndGet(byte[] outputBytes, int getCount);

        private const int DeviceResponseTimeout = 1000;

        public static void SendAndGetCommon(Command command)
        {
            if (command == null || command.Port.IsNullOrEmpty() || command.Ip.IsNullOrEmpty() || command.DatagramRequest.Length == 0) return;

            //#if DEBUG
            //            command.AddResponse(new byte[]{11,11,11,11,11,11,11,11,11,11}, 9999.ToString());
            //            return;
            //#endif

            Console.WriteLine("\n");

            switch (command.Protocol)
            {
                case ProtocolType.Udp:
                    SendAndGetUdp(command);
                    break;
                case ProtocolType.Tcp:
                    SendAndGetTcp(command);
                    break;
                default:
                    Console.WriteLine($"\nSendAndGetCommon: unknown Protocol {command.Protocol}");
                    break;
            }
        }
        private static void SendAndGetTcp(Command command)
        {
#if DEBUG
            Console.WriteLine($"\nSendAndGetTcp: Send: {command.Ip}:{command.Port} :\n" +
                              $"Request:  {BitConverter.ToString(command.DatagramRequest)}");
#endif

            // ReSharper disable once ConvertToUsingDeclaration
            using (var socketTcp = new TcpClient())
            {
                IPAddress.TryParse(command.Ip, out var ipAddress);
                socketTcp.Connect(new IPEndPoint(ipAddress ?? throw new InvalidOperationException(), int.Parse(command.Port)));

                if ((!socketTcp.Client.Connected || !socketTcp.Connected)) return;

                var streamTcp = socketTcp.GetStream();

                try
                {
                    streamTcp.Write(command.DatagramRequest, 0, command.DatagramRequest.Length);

                    if (!command.NeedResponse) return;

                    var _try = 3;

                    do
                    {
                        Task.Delay(1000);

                        if (streamTcp.CanRead)
                        {
                            var bytesTcp = new byte[1024];
                            var redTcp = streamTcp.Read(bytesTcp, 0, 1024);
                            var result = new byte[redTcp];

                            bytesTcp.CopyTo(result, redTcp);
                            command.AddResponse(result, command.Port);
#if DEBUG
                            Console.WriteLine($"Response: {BitConverter.ToString(result)}\n");
#endif
                            socketTcp.Close();
                            streamTcp.Close();

                            return;
                        }
                    } while (--_try > 0);

                    command.AddResponse(new byte[] { 0xff }, "0xff");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"SendAndGetCommon: Send: write operation fail: {e.Message}!");
                }
            }
        }
        private static void SendAndGetUdp(Command command)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                if (command.NeedResponse)
                {
                    StartListenUdp(command, cancellationTokenSource);
                }

                SendUdp(command);
                Thread.Sleep(DeviceResponseTimeout);
                cancellationTokenSource.Cancel();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void SendUdp(Command command)
        {
            using (var client = new UdpClient())
            {
                IPAddress.TryParse(command.Ip, out var ipAddress);

                var ipEndPoint = new IPEndPoint(ipAddress ?? throw new InvalidOperationException(), int.Parse(command.Port));

#if DEBUG
                Console.WriteLine($"SendUdp: Send:\n" +
                                  $"\tto {ipAddress}:{command.Port}:\n" +
                                  $"\tbytes = {BitConverter.ToString(command.DatagramRequest)}");
#endif

                client.Client.EnableBroadcast = true;
                client.Send(command.DatagramRequest, command.DatagramRequest.Length, ipEndPoint);
                client.Close();
            }
        }

        private static void StartListenUdp(Command command, CancellationTokenSource cancellationTokenSource)
        {
            var udpServerListenPorts = new List<int>() { 9999, 1021, 1022, 5011, 5012, 5013, 5016, };

            udpServerListenPorts.ForEach(port =>
            {
                Task.Run(() =>
                {

                    var udpClient = new UdpClient(port);

                    cancellationTokenSource.Token.Register(() =>
                    {
                        udpClient.Dispose();
                    });

                    udpClient.BeginReceive(RequestCallback, new object());

#if DEBUG
                    Console.WriteLine($"ListenUdp: UDP: Listening {command.Ip}:{port}");
#endif

                    return;

                    void RequestCallback(IAsyncResult result)
                    {
                        try
                        {
                            Receive(result);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"EX: ListenUdp: UDP: {e.Message}\n");
                        }
                        finally
                        {
                            if (!cancellationTokenSource.IsCancellationRequested)
                            {
                                udpClient.BeginReceive(RequestCallback, new object());
                            }
                        }
                    }

                    void Receive(IAsyncResult result)
                    {
                        var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (udpClient.Client == null) return;

                        var bytes = udpClient.EndReceive(result, ref remoteIpEndPoint);

                        if (IsSentByServer(remoteIpEndPoint)) return;

#if DEBUG
                        Console.WriteLine($"ListenUdp: UDP: Response: {BitConverter.ToString(bytes)}");
#endif

                        command.AddResponse(bytes, remoteIpEndPoint!.Port.ToString());
                    }

                    bool IsSentByServer(IPEndPoint remoteIpEndPoint)
                    {
                        var localAddresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;

                        return localAddresses.Any(i =>
                                   remoteIpEndPoint.AddressFamily == i.AddressFamily &&
                                   remoteIpEndPoint.Address.Equals(i));
                    }
                }, cancellationTokenSource.Token);
            });

            Thread.Sleep(10);
        }
    }
}
