using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Extensions;

namespace CommandTransmitter.Network
{
    public class ServerIpDiscover
    {
        public static string ServerIpPort = "localhost:56126";

        private const string GoogleAppScriptUpdateHomeIp = "https://script.google.com/macros/s/AKfycbxCf8Oyer4pf1SlFkfBclVMl-cSdcii9TSOxPitEm6R4rUDh-SuWgQe2x1Cu5gl4sbzBQ/exec";
        private const string CommandCenterEndPoint = "api/CommandCenter/ConnectWorker/password=qwerty123$";

        public static async Task<string> GetServerIp()
        {
            var serverAddress = await Get();

            if (serverAddress.IsNullOrEmpty()) { return null; }

            return serverAddress;

            async Task<string> Get()
            {
                var tries = 3;

                using (var httpClient = new HttpClient())
                {

                    httpClient.Timeout = TimeSpan.FromSeconds(12);

                    HttpResponseMessage response = null;
                    var cancelToken = new CancellationTokenSource();

                    cancelToken.CancelAfter(10000);

                    do
                    {
                        try
                        {
                            response = await httpClient.GetAsync($"{GoogleAppScriptUpdateHomeIp}?password=12345678", cancelToken.Token);

                            response.EnsureSuccessStatusCode();

                            var responseBody = await response.Content.ReadAsStringAsync();

                            Console.WriteLine($"Get Ip from Google App Script: {responseBody}");
                            
                            ServerIpPort = responseBody;

#if DEBUG
                            return $"ws://localhost:56126/{CommandCenterEndPoint}";
#else
                            return $"ws://{ServerIpPort}/{CommandCenterEndPoint}";
#endif 
                        }
                        catch (HttpRequestException e)
                        {
                            if (response?.StatusCode != null && (int)response.StatusCode == 429)
                            {
                                continue;
                            }

                            Console.WriteLine($"Request Error: {e.Message}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"An unexpected error occurred: {e.Message}");
                        }

                        tries--;
                    }
                    while (tries > 0);
                }

                return "";
            }
        }
    }
}
