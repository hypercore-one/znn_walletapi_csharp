using System.ComponentModel.DataAnnotations;
using Zenon;

namespace ZenonWalletApi.Options
{
    public class NodeOptions
    {
        public const string Node = "Api:Node";

        public const string DefaultNodeUrl = "ws://127.0.0.1:35998";
        public const int DefaultMaxPoWThreads = 5;

        [Required]
        public required string NodeUrl { get; set; } = DefaultNodeUrl;
        [Required]
        public required int ChainId { get; set; } = Constants.ChainId;
        [Required]
        public required int ProtocolVersion { get; set; } = Constants.ProtocolVersion;
        [Range(1, 100)]
        public int MaxPoWThreads { get; set; } = DefaultMaxPoWThreads;
    }
}
