using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Pavlok
{
    public interface IPavlokStimuliClient : IDisposable
    {
        /// <summary>
        /// Generates a vibration stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <returns></returns>
        Task SendVibration(int value);

        /// <summary>
        /// Generates a beep stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <returns></returns>
        Task SendBeep(int value);

        /// <summary>
        /// Generates a shock stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <returns></returns>
        Task SendShock(int value);
    }

    public class PavlokStimuliStimuliClient : IPavlokStimuliClient
    {
        // TODO:
        // /api/v1/me
        // /api/v2/fitness/steps?from=2021-12-14&to=2021-12-14
        // /api/v1/stimuli/pattern/

        private readonly HttpClient _client;
        private readonly string _authenticationHeader;
        private readonly bool _disposeHttpClient;

        public PavlokStimuliStimuliClient(HttpClient client, string authenticationHeader, bool disposeHttpClient = true)
        {
            _client = client;
            _authenticationHeader = authenticationHeader;
            _disposeHttpClient = disposeHttpClient;
        }

        /// <summary>
        /// Generates a vibration stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <returns></returns>
        public async Task SendVibration(int value)
        {
            await SendStimuli("vibration", value);
        }

        /// <summary>
        /// Generates a beep stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <returns></returns>
        public async Task SendBeep(int value)
        {
            await SendStimuli("beep", value);
        }

        /// <summary>
        /// Generates a shock stimulus to the user's Pavlok device.
        /// </summary>
        /// <param name="value">The value intensifier for the stimulus. Allowed values: 1-255</param>
        /// <returns></returns>
        public async Task SendShock(int value)
        {
            await SendStimuli("shock", value);
        }

        private async Task SendStimuli(string type, int value)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/stimuli/{type}/{value}")
            {
                Headers = { Authorization = AuthenticationHeaderValue.Parse(_authenticationHeader) }
            };

            using var responseMessage = await _client.SendAsync(request);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var body = await responseMessage.Content.ReadAsStringAsync();
                throw new PavlokApiException(body, responseMessage.StatusCode);
            }
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