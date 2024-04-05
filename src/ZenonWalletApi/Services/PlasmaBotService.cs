using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Services
{
    public interface IPlasmaBotService
    {
        Task<IResult> FuseAsync(Address address);
    }

    public class PlasmaBotOptions
    {
        public const string PlasmaBot = "Api:Utilities:PlasmaBot";

        [Required, Url]
        public required string ApiUrl { get; set; }
        [Required]
        public required string ApiKey { get; set; }
    }

    public class PlasmaBotService : IPlasmaBotService
    {
        public static Uri CombineUri(string baseUri, string relativeOrAbsoluteUri)
        {
            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri);
        }

        public PlasmaBotService(ILogger<PlasmaBotService> logger, IOptions<PlasmaBotOptions> options, HttpClient httpClient)
        {
            Logger = logger;
            Options = options.Value;
            HttpClient = httpClient;
            HttpClient.BaseAddress = new Uri(Options.ApiUrl);
            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Options.ApiKey}");
        }

        private ILogger<PlasmaBotService> Logger { get; }

        private PlasmaBotOptions Options { get; }

        private HttpClient HttpClient { get; }

        public async Task<IResult> FuseAsync(Address address)
        {
            var response = await HttpClient.PostAsJsonAsync("fuse", new { address = address.ToString() });

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string? message = null;
                int statusCode = (int)response.StatusCode;

                try
                {
                    var json = await response.Content.ReadFromJsonAsync<JToken>();

                    if (json != null)
                    {
                        message = json.Value<string?>("message");
                    }
                }
                catch (Exception e)
                {
                    message = "Failed to parse fuse response";
                    statusCode = (int)HttpStatusCode.InternalServerError;

                    Logger.LogWarning(e, message);
                }

                return Results.Problem(message, null, statusCode);
            }
            else
            {
                return Results.Ok();
            }
        }
    }
}
