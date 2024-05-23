using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Zenon;

namespace ZenonWalletApi.Options
{
    public class WalletOptions
    {
        public const string Wallet = "Api:Wallet";

        public static string DefaultWalletPath = ZdkPaths.Default.Wallet;
        public static string DefaultWalletName = "api";

        [Required]
        public required string Path { get; set; } = DefaultWalletPath;
        [Required]
        public required string Name { get; set; } = DefaultWalletName;
        [Range(3, 10), AllowNull]
        public int? EraseLimit { get; set; }
    }
}
