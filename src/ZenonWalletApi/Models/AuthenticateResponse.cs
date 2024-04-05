namespace ZenonWalletApi.Models
{
    public class AuthenticateResponse
    {
        public AuthenticateResponse(User user, string token)
        {
            Id = user.Id;
            Username = user.Username;
            Token = token;
        }

        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
