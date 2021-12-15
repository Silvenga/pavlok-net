using Newtonsoft.Json;

namespace Pavlok.Models
{
    public class LoginResult
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; } = null!;

        [JsonProperty("token_type")]
        public string TokenType { get; set; } = null!;

        [JsonProperty("expires_in")]
        public long ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; } = null!;

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        public string AuthenticationHeader => $"{TokenType} {AccessToken}";
    }
}