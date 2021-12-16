using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Pavlok.Bridge.Controllers
{
    [ApiController, Route("/v1/stimuli")]
    public class StimuliController : Controller
    {
        private readonly PavlokLoginManager _loginManager;

        public StimuliController(PavlokLoginManager loginManager)
        {
            _loginManager = loginManager;
        }

        [HttpPost("beep")]
        public async Task<IActionResult> SendBeep([FromQuery] int value = 128)
        {
            return await Send(client => client.SendBeep(value));
        }

        [HttpPost("vibration")]
        public async Task<IActionResult> SendVibration([FromQuery] int value = 128)
        {
            return await Send(client => client.SendVibration(value));
        }

        [HttpPost("shock")]
        public async Task<IActionResult> SendShock([FromQuery] int value = 128)
        {
            return await Send(client => client.SendShock(value));
        }

        private async Task<IActionResult> Send(Func<IPavlokStimuliClient, Task> func)
        {
            if (!TryGetAuthenticationHeader(out var username, out var password))
            {
                return Unauthorized();
            }

            var loginResult = await _loginManager.GetLogin(username, password);
            switch (loginResult)
            {
                case PavlokLoginManager.SuccessfulLoginLease successful:
                    using (var pavlokStimuliClient = PavlokClientFactory.CreateStimuliClient(successful.AuthenticationHeader))
                    {
                        await func.Invoke(pavlokStimuliClient);
                    }

                    return Ok();
                case PavlokLoginManager.FailedLoginLease failed:
                    return BadRequest(new
                    {
                        BackendStatusCode = failed.StatusCode
                    });
            }

            return BadRequest();
        }

        private bool TryGetAuthenticationHeader([NotNullWhen(true)] out string? username, [NotNullWhen(true)] out string? password)
        {
            var header = Request.Headers.Authorization.FirstOrDefault();
            if (header != null)
            {
                var parameterParts = header.Split(" ");
                if (parameterParts.Length == 2
                    && parameterParts[0].Equals("basic", StringComparison.OrdinalIgnoreCase))
                {
                    var base64Bytes = Convert.FromBase64String(parameterParts[1]);
                    var plainText = Encoding.UTF8.GetString(base64Bytes);
                    var decodedParts = plainText.Split(new[] { ':' }, 2);
                    if (decodedParts.Length == 2)
                    {
                        username = decodedParts[0];
                        password = decodedParts[1];
                        return true;
                    }
                }
            }

            username = default;
            password = default;
            return false;
        }
    }
}