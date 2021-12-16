using System;
using System.Net.Http;

namespace Pavlok
{
    public static class PavlokClientFactory
    {
        public const string DefaultBaseAddress = "https://pavlok-mvp.herokuapp.com";

        public static IPavlokStimuliClient CreateStimuliClient(string authenticationHeader, string baseAddress = DefaultBaseAddress)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
            ApplyUserAgent(httpClient);

            return CreateStimuliClient(httpClient, authenticationHeader);
        }

        public static IPavlokStimuliClient CreateStimuliClient(HttpClient httpClient, string authenticationHeader)
        {
            return new PavlokStimuliStimuliClient(httpClient, authenticationHeader);
        }

        public static IPavlokLoginClient CreateLoginClient(string baseAddress = DefaultBaseAddress)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };
            ApplyUserAgent(httpClient);

            return CreateLoginClient(httpClient);
        }

        public static IPavlokLoginClient CreateLoginClient(HttpClient httpClient)
        {
            return new PavlokLoginClient(httpClient);
        }

        private static void ApplyUserAgent(HttpClient httpClient)
        {
            // TODO make this dynamic
            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Pavlok.NET/1.0.0 (https://github.com/Silvenga/pavlok-net)");
        }
    }
}