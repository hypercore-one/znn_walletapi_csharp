namespace ZenonWalletApi.Models
{
    /// <summary>
    /// Determines the mode for generating plasma.
    /// </summary>
    public enum PlasmaMode
    {
        /// <summary>
        /// Proof-of-Work
        /// </summary>
        PoW,
        /// <summary>
        /// Automatically fuse
        /// </summary>
        Fuse,
        /// <summary>
        /// Automatically fuse or Proof-of-Work
        /// </summary>
        Both
    }
}
