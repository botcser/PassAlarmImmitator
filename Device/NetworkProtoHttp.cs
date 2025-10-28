using Casualbunker.Server.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Extensions;

namespace Device
{
    public class NetworkProtoHttp: INetworkProtoDual
    {
        public static NetworkProtoHttp Instance => _instance ??= new NetworkProtoHttp();
        
        public string Ip { get; set; }

        private const string UrlServer = "http://commandcenter.runasp.net/api/commandcenter/ping";
        private const string UrlSend = 1 == 0 ? UrlDebug : UrlRelease;
        private const string UrlDebug = "http://localhost:56126/api/commandcenter/SendAndGetJson";
        private const string UrlRelease = "http://commandcenter.runasp.net/api/commandcenter/SendAndGetJson";

        private static NetworkProtoHttp _instance;

        private IPEndPoint IpEndPoint;

        public NetworkProtoHttp() { }

        public NetworkProtoHttp(string ip, int port)
        {
            IPAddress.TryParse(ip, out var ipAddress);

            if (ipAddress != null)
            {
                IpEndPoint = new IPEndPoint(ipAddress, port);
            }
            else
            {
                Console.WriteLine($"EX: ServerHelper: wrong IP!");
            }
        }

        public void Send(Command command, Action<string> onComplete = null)
        {
            if (!Connect()) return;
            
            Task.Run(async () =>
            {
#if DEBUG
                Console.WriteLine($"\nServerHelper: Send: {IpEndPoint.Address}:{IpEndPoint.Port} :\n" +
                                  $"Request:  {BitConverter.ToString(command.DatagramRequest)}");
#endif
                var response = await HttpPost(command);

#if DEBUG
                Console.WriteLine($"Response: {response}\n");
#endif

                onComplete?.Invoke(response);
            });
        }
        
        public void SendTcp(byte[] bytes, string ip, int port, ProtocolType protocol = ProtocolType.Tcp, Action<string> onComplete = null)
        {
            IPAddress.TryParse(ip, out var ipAddress);
            IpEndPoint = new IPEndPoint(ipAddress, port);

            Send(new Command(bytes, IpEndPoint.Address.ToString(), IpEndPoint.Port.ToString(), protocol) { NeedResponse = true }, onComplete);
        }

        public void Disconnect()
        {
            return;
        }

        public bool Send(byte[] bytes)
        {
            try
            {
                Send(new Command(bytes, IpEndPoint.Address.ToString(), IpEndPoint.Port.ToString(), ProtocolType.Tcp) { NeedResponse = true });

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"NetworkProtoCommonDual: Send: write operation fail: {e.Message}!");

                return false;
            }
        }

        public bool Connect()
        {
            var result = "";
            var done = false;

            try
            {
                Task.Run(async () =>
                {
                    result = await HttpGet();
                    done = true;
                });

                Thread.Sleep(2000);
                if (result.IsNullOrEmpty() && !done) Thread.Sleep(2000);
                if (result.IsNullOrEmpty() && !done) Thread.Sleep(2000);
            }
            catch (Exception e)
            {
                Console.WriteLine($"EX: Connect: {e.Message}");
            }

            return result.ToLower().Contains("success");
        }

        public byte[] Get(int count)
        {
            return Array.Empty<byte>();
        }

        public byte[] SendAndGet(byte[] bytes, int getCount = 0)
        {
            try
            {
                var result = "";
                var done = false;

                Send(new Command(bytes, IpEndPoint.Address.ToString(), IpEndPoint.Port.ToString(), ProtocolType.Tcp) { NeedResponse = true },
                    (string s) =>
                    {
                        result = s.ToString();
                        done = true;
                    });
                
                Thread.Sleep(2000);

                if (result.IsNullOrEmpty() && !done) Thread.Sleep(2000);
                if (result.IsNullOrEmpty() && !done) Thread.Sleep(2000);
                if (result.IsNullOrEmpty() && !done) Thread.Sleep(2000); 

                return result.IsNullOrEmpty() ? Array.Empty<byte>() : JsonGeneric.FromJson<Command>(result).GetResponse(0);
            }
            catch (Exception e)
            {
                Console.WriteLine($"NetworkProtoCommonDual: Send: write operation fail: {e.Message}!");

                return Array.Empty<byte>();
            }
        }
        
        private async Task<string> HttpPost(Command command, TaskCompletionSource done = null)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(12000000);

                    var values = new Dictionary<string, string>
                    {
                        { "password", "qwerty123$" },
                        { "commandJson", JsonGeneric.ToJson(command) }
                    };
                    var content = new FormUrlEncodedContent(values);
                    var response = await httpClient.PostAsync(UrlSend, content);

                    response.EnsureSuccessStatusCode();

                    var result = await response.Content.ReadAsStringAsync();

                    done?.SetResult();

                    return result;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An unexpected error occurred: {e.Message}");
                }
            }

            return null;
        }

        private async Task<string> HttpGet()
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(12000000);

                    var response = await httpClient.GetAsync(UrlServer);

                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"EX: Get: An unexpected error occurred: {e.Message}");
                }
            }

            return "";
        }
    }
}
