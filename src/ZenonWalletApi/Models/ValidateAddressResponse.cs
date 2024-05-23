namespace ZenonWalletApi.Models
{
    public record ValidateAddressResponse(bool IsValid, bool IsEmbedded)
    {
        /// <summary>
        /// Indicates whether the address is valid
        /// </summary>
        public bool IsValid { get; } = IsValid;

        /// <summary>
        /// Indicates whether the address is embedded
        /// </summary>
        public bool IsEmbedded { get; } = IsEmbedded;
    }
}
