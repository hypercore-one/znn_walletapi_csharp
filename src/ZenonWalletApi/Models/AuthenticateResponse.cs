namespace ZenonWalletApi.Models
{
    public record class AuthenticateResponse(Guid Id, string Username, string Token)
    {
        /// <summary>
        /// The unique id of the user
        /// </summary>
        public Guid Id { get; } = Id;

        /// <summary>
        /// The username of the user
        /// </summary>
        public string Username { get; } = Username;

        /// <summary>
        /// The authorized token of the user
        /// </summary>
        public string Token { get; } = Token;
    }
}
