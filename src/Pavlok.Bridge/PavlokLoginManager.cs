using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pavlok.Models;

namespace Pavlok.Bridge
{
    public class PavlokLoginManager : IDisposable
    {
        private readonly ConcurrentDictionary<Login, LoginResult> _loginResults = new();
        private readonly IPavlokLoginClient _loginClient;
        private readonly ILogger<PavlokLoginManager> _logger;

        public PavlokLoginManager(IPavlokLoginClient loginClient, ILogger<PavlokLoginManager> logger)
        {
            _loginClient = loginClient;
            _logger = logger;
        }

        public async Task<LoginLease> GetLogin(string username, string password)
        {
            try
            {
                var login = new Login(username, password);
                if (!_loginResults.TryGetValue(login, out var loginResult)
                    || HasExpired(loginResult))
                {
                    _logger.LogInformation($"Fetching authentication token for user '{username}'.");

                    loginResult = await _loginClient.Login(login.Username, login.Password);
                    _loginResults.TryAdd(login, loginResult);
                }
                else
                {
                    _logger.LogInformation($"Using cached authentication token for user '{username}'.");
                }

                return new SuccessfulLoginLease(username, loginResult.AuthenticationHeader);
            }
            catch (PavlokApiException e)
            {
                _logger.LogWarning(e, $"Failed to fetch authentication token for user '{username}' (Status Code: {e.StatusCode}, Body: '{e.Body}').");
                return new FailedLoginLease(username, e.StatusCode);
            }
        }

        private static bool HasExpired(LoginResult loginResult)
        {
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(loginResult.CreatedAt + loginResult.ExpiresIn);
            return DateTimeOffset.Now >= expiresAt;
        }

        private record Login(string Username, string Password);

        public abstract record LoginLease(string Username);

        public record SuccessfulLoginLease(string Username, string AuthenticationHeader) : LoginLease(Username);

        public record FailedLoginLease(string Username, HttpStatusCode? StatusCode) : LoginLease(Username);

        public void Dispose()
        {
            _loginClient.Dispose();
        }
    }
}