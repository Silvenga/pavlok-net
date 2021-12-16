using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Pavlok.Models;

namespace Pavlok.Bridge
{
    public class PavlokLoginManager : IDisposable
    {
        private readonly ConcurrentDictionary<Login, LoginResult> _loginResults = new();
        private readonly IPavlokLoginClient _loginClient;

        public PavlokLoginManager(IPavlokLoginClient loginClient)
        {
            _loginClient = loginClient;
        }

        public async Task<LoginLease> GetLogin(string username, string password)
        {
            try
            {
                var login = new Login(username, password);
                if (!_loginResults.TryGetValue(login, out var loginResult)
                    || HasExpired(loginResult))
                {
                    loginResult = await _loginClient.Login(login.Username, login.Password);
                    _loginResults.TryAdd(login, loginResult);
                }

                return new SuccessfulLoginLease(username, loginResult.AuthenticationHeader);
            }
            catch (HttpRequestException e)
            {
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