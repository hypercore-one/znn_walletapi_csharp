using Serilog;
using ZenonWalletApi;
using ZenonWalletApi.Infrastructure.Configurations;

Log.Logger = SerilogConfigurator.CreateLogger();

try
{
    Log.Logger.Information("Starting up");
    using var webHost = CreateWebHostBuilder(args).Build();
    await webHost.RunAsync();
}
catch (Exception ex)
{
    Log.Logger.Fatal(ex, "Application start-up failed");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

static IHostBuilder CreateWebHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });