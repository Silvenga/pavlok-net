using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pavlok.Models;

namespace Pavlok
{
    public interface IPavlokLoginClient : IDisposable
    {
        /// <summary>
        /// Login via resource owner password grant flow (undocumented, but appears to work :shrug:).
        /// Internally, the api appears to use the Ruby Doorkeeper library.
        /// </summary>
        Task<LoginResult> Login(string username, string password);
    }

    public class PavlokLoginClient : IPavlokLoginClient
    {
        private readonly HttpClient _client;
        private readonly bool _disposeHttpClient;

        public PavlokLoginClient(HttpClient client, bool disposeHttpClient = true)
        {
            _client = client;
            _disposeHttpClient = disposeHttpClient;
        }

        /// <summary>
        /// Login via resource owner password grant flow (undocumented, but appears to work :shrug:).
        /// Internally, the api appears to use the Ruby Doorkeeper library.
        /// </summary>
        public async Task<LoginResult> Login(string username, string password)
        {
            var requestBody = GetJsonBody(new Dictionary<string, string>
            {
                { "username", username },
                { "password", password },
                { "grant_type", "password" }
            });

            using var responseMessage = await _client.PostAsync("/oauth/token", requestBody);
            return await GetResponseBody<LoginResult>(responseMessage);
        }

        private static StringContent GetJsonBody<T>(T body)
        {
            return new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json"
            );
        }

        private static async Task<T> GetResponseBody<T>(HttpResponseMessage responseMessage)
        {
            responseMessage.EnsureSuccessStatusCode();

            var json = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json)!;
        }

        public void Dispose()
        {
            if (_disposeHttpClient)
            {
                _client.Dispose();
            }
        }
    }
}