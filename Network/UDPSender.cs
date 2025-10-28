using System.Net;
using System.Net.Sockets;

namespace IRAPROM.MyCore.MyNetwork
{
    public class UDPSender : IUDPSend
    {
        public static UDPSender Instance => _instance ??= new UDPSender();

        private static object _lock = new object();
        private static UDPSender _instance;

        public void Send(byte[] bytes, int port, string ip = "192.168.16.255", TaskCompletionSource done = null)
        {
#if CLIENT_DISABLED
            if (port == ApplMain.Loader_UDPPortRetransmission)                                          
            {
                return;
            }
#endif

            if (port == App.Loader_UDPPortRetransmission)                                          // TODO: make Client remote
            {
                ip = "127.0.0.1";
            }

#if DEBUG
            Console.WriteLine($"UDPSender: Send:\n" +
                              $"\tto {ip}:{port}:\n" +
                              $"\t\tbytes = {BitConverter.ToString(bytes)}\n");
#endif

#if USE_COMMAND_CENTER
            if (port != ApplMain.Loader_UDPPortRetransmission)
            {
                NetworkProtoHttp.Instance.SendTcp(bytes, ip, port, ProtocolType.Udp, (string response) =>
                {
                    if (!response.IsNullOrEmpty())
                    {
                        lock (_lock)
                        {
                            var commandCompleted = JsonGeneric.FromJson<Command>(response);
                            
                            for (var i = 0; i < commandCompleted.ResponsePorts.Count; i++)
                            {
                                var remoteIpEndPoint = new IPEndPoint(IPAddress.Any, int.Parse(commandCompleted.ResponsePorts[i]));

                                UdpListenerServer.ParseResponse(commandCompleted.GetResponse(i), remoteIpEndPoint);
                            }
                        }
                    }

                    done?.SetResult();
                });

                return;
            }
#endif

            using (var client = new UdpClient())
            {                                                                                // TODO
                IPAddress.TryParse(ip, out var ipAddress);
                
                var ipEndPoint = new IPEndPoint(ipAddress, port);

                client.Client.EnableBroadcast = true;
                client.Send(bytes, bytes.Length, ipEndPoint);
                client.Close();

                done?.SetResult();
            }
        }
        
        public void Send(byte[] bytes, int port, string ip)
        {
            using (var client = new UdpClient())
            {
                var ipAddress = System.Net.IPAddress.Parse(ip);
                var ipEndPoint = new IPEndPoint(ipAddress, port);

                client.Send(bytes, bytes.Length, ipEndPoint);
                client.Close();
            }
        }

    }

    public interface IUDPSend
    {
        public void Send(byte[] bytes, int port, string ip = "192.168.16.255", TaskCompletionSource done = null);
    }
}
