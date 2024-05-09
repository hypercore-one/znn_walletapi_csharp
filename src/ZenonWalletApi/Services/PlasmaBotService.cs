using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net;
using Zenon.Model.Primitives;
using ZenonWalletApi.Options;

namespace ZenonWalletApi.Services
{
    public interface IPlasmaBotService
    {
        Task<DateTime?> GetExpirationAsync(Address address, CancellationToken cancellationToken = default);
        Task FuseAsync(Address address, CancellationToken cancellationToken = default);
    }

    internal class PlasmaBotService : IPlasmaBotService
    {
        private const string DateTimeFormat = "yyyy-dd-MM HH:mm:ss";

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

        public async Task<DateTime?> GetExpirationAsync(Address address, CancellationToken cancellationToken = default)
        {
            var response = await HttpClient.GetAsync($"expiration/{address}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                try
                {
                    var json = await response.Content.ReadFromJsonAsync<JToken>(cancellationToken);

                    if (json != null)
                    {
                        var data = json.Value<string?>("data");

                        if (data != null)
                        {
                            return DateTime.ParseExact(data, DateTimeFormat, CultureInfo.InvariantCulture);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogWarning(e, "Failed to parse plasma-bot expiration response");

                    throw;
                }
            }

            return null;
        }

        public async Task FuseAsync(Address address, CancellationToken cancellationToken = default)
        {
            Logger.LogDebug($"Fusing plasma for address: {address}");

            var response = await HttpClient.PostAsJsonAsync("fuse", new { address = address.ToString() }, cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                string? message = null;
                var statusCode = response.StatusCode;

                try
                {
                    var json = await response.Content.ReadFromJsonAsync<JToken>(cancellationToken);

                    if (json != null)
                    {
                        message = json.Value<string?>("message");
                    }
                }
                catch (Exception e)
                {
                    message = "Failed to parse plasma-bot fuse response";
                    statusCode = HttpStatusCode.InternalServerError;

                    Logger.LogWarning(e, message);
                }

                throw new HttpRequestException(message, null, statusCode);
            }
        }
    }
}
