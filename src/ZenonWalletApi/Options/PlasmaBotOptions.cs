using System.ComponentModel.DataAnnotations;

namespace ZenonWalletApi.Options
{
    public class PlasmaBotOptions
    {
        public const string PlasmaBot = "Api:Utilities:PlasmaBot";

        public const string DefaultApiUrl = "https://zenonhub.io/api/utilities/plasma-bot/";

        [Required, Url]
        public required string ApiUrl { get; set; } = DefaultApiUrl;
        [Required]
        public required string ApiKey { get; set; }
    }
}
