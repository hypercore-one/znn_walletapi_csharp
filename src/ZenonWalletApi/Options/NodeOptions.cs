using System.ComponentModel.DataAnnotations;
using Zenon;
using ZenonWalletApi.Models;

namespace ZenonWalletApi.Options
{
    public class NodeOptions
    {
        public const string Node = "Api:Node";

        public const string DefaultNodeUrl = "ws://127.0.0.1:35998";
        public const int DefaultMaxPoWThreads = 5;
        public const int DefaultMinQsrThreshold = 100;

        [Required]
        public required string NodeUrl { get; set; } = DefaultNodeUrl;
        [Required]
        public required int ChainId { get; set; } = Constants.ChainId;
        [Required]
        public required int ProtocolVersion { get; set; } = Constants.ProtocolVersion;

        /// <summary>
        /// The maximum amount of Proof-of-Work threads that can run simultaneously.
        /// </summary>
        /// <remarks>
        /// Threads that require PoW will be blocked when the <see cref="MaxPoWThreads"/> is reached.
        /// </remarks>
        [Range(1, 100)]
        public int MaxPoWThreads { get; set; } = DefaultMaxPoWThreads;

        /// <summary>
        /// Indicates how plasma is generated when the minimum QSR threshold is not reached.
        /// </summary>
        public PlasmaMode PlasmaMode { get; set; } = PlasmaMode.PoW;

        /// <summary>
        /// The minimum amount of QSR that must be fused to an address.
        /// </summary>
        /// <remarks>
        /// This option is ignored when <see cref="PlasmaMode"/> is <code>PlasmaMode.PoW</code>.
        /// </remarks>
        [Range(1, 5000)]
        public int MinQsrThreshold { get; set; } = DefaultMinQsrThreshold;

        /// <summary>
        /// The maximum amount of time to wait for the fusion to complete.
        /// </summary>
        /// <remarks>
        /// This option is ignored when <see cref="PlasmaMode"/> is <code>PlasmaMode.PoW</code>.
        /// </remarks>
        public TimeSpan FuseTimeout { get; set; } = TimeSpan.FromMinutes(1);
    }
}
