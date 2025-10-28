using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Device
{
    public class HttpHelper
    {
        public static async Task<string> Get(string url)
        {
            var tries = 3;

            using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(12000) })
            {
                HttpResponseMessage response = null;

                do
                {
                    try
                    {
                        response = await httpClient.GetAsync(url);
                        response.EnsureSuccessStatusCode();

                        var responseBody = await response.Content.ReadAsStringAsync();

                        Console.WriteLine($"HttpHelper: Get: {responseBody}");

                        return responseBody;
                    }
                    catch (HttpRequestException e)
                    {
                        if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            await Task.Delay(3000);
                            continue;
                        }

                        Console.WriteLine($"EX: HttpHelper: Get: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"EX: HttpHelper: Get: {e.Message}");
                    }

                    tries--;
                } while (tries > 0);
            }

            return "";
        }

        public static async Task<string> Post(string url, Dictionary<string, string> args)
        {
            var tries = 3;

            using (var httpClient = new HttpClient() { Timeout = TimeSpan.FromMilliseconds(12000) })
            {
                HttpResponseMessage response = null;

                do
                {
                    try
                    {
                        var content = new FormUrlEncodedContent(args);

                        response = await httpClient.PostAsync(url, content);
                        response.EnsureSuccessStatusCode();

                        var responseBody = await response.Content.ReadAsStringAsync();

                        Console.WriteLine($"HttpHelper: Post: {responseBody}");

                        return responseBody;
                    }
                    catch (HttpRequestException e)
                    {
                        if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            await Task.Delay(3000);
                            continue;
                        }

                        Console.WriteLine($"EX: HttpHelper: Post: {e.Message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"EX: HttpHelper: Post: {e.Message}");
                    }

                    tries--;
                } while (tries > 0);
            }

            return "";
        }
    }
}
