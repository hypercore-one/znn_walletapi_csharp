namespace ZenonWalletApi.Models
{
    public record GetBotPlasmaExpirationResponse(DateTime? Expiration)
    {
        /// <summary>
        /// The fusion expiration
        /// </summary>
        public DateTime? Expiration { get; } = Expiration;
    }
}
