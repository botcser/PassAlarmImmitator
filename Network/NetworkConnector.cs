using Exception = System.Exception;

#nullable disable

namespace Extensions.Network
{
    public class NetworkConnector
    {
        private const string GoogleAppScriptUpdateHomeIp = "https://script.google.com/macros/s/AKfycbxCf8Oyer4pf1SlFkfBclVMl-cSdcii9TSOxPitEm6R4rUDh-SuWgQe2x1Cu5gl4sbzBQ/exec";

        private readonly CancellationToken _cancellationToken;

        public NetworkConnector(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
        }
        
        public async Task<string> GetServerIp()
        {
            //return IPEndPoint.Parse("0.0.0.0:56126");

            var serverAddress = await Get();

            if (serverAddress.IsNullOrEmpty()) { return null; }

            return serverAddress;

            async Task<string> Get()
            {
                var tries = 3;
                using var httpClient = new HttpClient();

                httpClient.Timeout = TimeSpan.FromSeconds(12);

                HttpResponseMessage response = null;

                do
                {
                    try
                    {
                        response = await httpClient.GetAsync($"{GoogleAppScriptUpdateHomeIp}?password=12345678", _cancellationToken);

                        response.EnsureSuccessStatusCode();

                        var responseBody = await response.Content.ReadAsStringAsync(_cancellationToken);

                        Console.WriteLine($"Get Ip from Google App Script: {responseBody}");

                        return responseBody;
                    }
                    catch (HttpRequestException e)
                    {
                        if ((int)response!.StatusCode == 429)
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

                return "";
            }
        }
    }
}
