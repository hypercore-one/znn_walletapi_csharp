using System.ComponentModel.DataAnnotations;
using ZenonWalletApi.Models;

namespace ZenonWalletApi.Options
{
    public class ApiOptions
    {
        public const string Api = "Api";

        [Required]
        public required IEnumerable<User> Users { get; set; }
    }
}
