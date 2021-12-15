using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pavlok.Models;

namespace Pavlok
{
    public interface IPavlokClient
    {
        /// <summary>
        /// Login via resource owner password grant flow (undocumented, but appears to work :shrug:).
        /// Internally, the api appears to use the Ruby Doorkeeper library.
        /// </summary>
        Task<LoginResult> Login(string username, string password);

        /// <summary>
        /// Generates a vibration stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <param name="loginResult"></param>
        /// <returns></returns>
        Task SendVibration(int value, LoginResult loginResult);

        /// <summary>
        /// Generates a beep stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <param name="loginResult"></param>
        /// <returns></returns>
        Task SendBeep(int value, LoginResult loginResult);

        /// <summary>
        /// Generates a shock stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <param name="loginResult"></param>
        /// <returns></returns>
        Task ShockBeep(int value, LoginResult loginResult);
    }

    public class PavlokClient : IPavlokClient
    {
        // TODO:
        // /api/v1/me
        // /api/v2/fitness/steps?from=2021-12-14&to=2021-12-14
        // /api/v1/stimuli/pattern/

        private readonly HttpClient _client;

        public PavlokClient(HttpClient client)
        {
            _client = client;
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

        /// <summary>
        /// Generates a vibration stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <param name="loginResult"></param>
        /// <returns></returns>
        public async Task SendVibration(int value, LoginResult loginResult)
        {
            await SendStimuli("vibration", value, loginResult);
        }

        /// <summary>
        /// Generates a beep stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <param name="loginResult"></param>
        /// <returns></returns>
        public async Task SendBeep(int value, LoginResult loginResult)
        {
            await SendStimuli("beep", value, loginResult);
        }

        /// <summary>
        /// Generates a shock stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <param name="loginResult"></param>
        /// <returns></returns>
        public async Task ShockBeep(int value, LoginResult loginResult)
        {
            await SendStimuli("shock", value, loginResult);
        }

        private async Task SendStimuli(string type, int value, LoginResult loginResult)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/stimuli/{type}/{value}")
            {
                Headers = { Authorization = AuthenticationHeaderValue.Parse(loginResult.AuthenticationHeader) }
            };

            using var responseMessage = await _client.SendAsync(request);
            responseMessage.EnsureSuccessStatusCode();
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
    }
}