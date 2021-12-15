using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Pavlok
{
    public class PavlokClientFactory
    {
        public static PavlokClient Create(string baseAddress = "https://pavlok-mvp.herokuapp.com")
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            // TODO make this dynamic
            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Pavlok.NET", "1.0.0"));
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("https://github.com/Silvenga/pavlok-net"));

            return new PavlokClient(httpClient);
        }
    }
}