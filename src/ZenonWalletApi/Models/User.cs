using System.Text.Json.Serialization;

namespace ZenonWalletApi.Models
{
    public class User
    {
        public required Guid Id { get; set; }
        public required string Username { get; set; }
        [JsonIgnore]
        public required string PasswordHash { get; set; }
        public required string[] Roles { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
