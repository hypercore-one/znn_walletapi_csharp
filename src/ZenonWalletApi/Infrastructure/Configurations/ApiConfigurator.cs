using FluentValidation;
using ZenonWalletApi.Features.AddWalletAccounts;
using ZenonWalletApi.Features.AuthenticateUser;
using ZenonWalletApi.Features.CancelPlasma;
using ZenonWalletApi.Features.FuseBotPlasma;
using ZenonWalletApi.Features.FusePlasma;
using ZenonWalletApi.Features.GetAccountInfo;
using ZenonWalletApi.Features.GetAutoReceiverStatus;
using ZenonWalletApi.Features.GetFusionInfo;
using ZenonWalletApi.Features.GetPlasmaInfo;
using ZenonWalletApi.Features.GetReceivedAccountBlocks;
using ZenonWalletApi.Features.GetUnreceivedAccountBlocks;
using ZenonWalletApi.Features.GetWalletStatus;
using ZenonWalletApi.Features.InitializeWallet;
using ZenonWalletApi.Features.LockWallet;
using ZenonWalletApi.Features.MapGetWalletAccountsEndpoint;
using ZenonWalletApi.Features.ReceiveTransfer;
using ZenonWalletApi.Features.RestoreWallet;
using ZenonWalletApi.Features.SendTransfer;
using ZenonWalletApi.Features.UnlockWallet;
using ZenonWalletApi.Features.ValidateAddress;
using ZenonWalletApi.Infrastructure.ExceptionHandlers;
using ZenonWalletApi.Infrastructure.Filters;
using ZenonWalletApi.Models.Converters;
using ZenonWalletApi.Models.Events;
using ZenonWalletApi.Options;
using ZenonWalletApi.Services;

namespace ZenonWalletApi.Infrastructure.Configurations
{
    public static class ApiConfigurator
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuration
            services.AddOptions<ApiOptions>()
                .BindConfiguration(ApiOptions.Api)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.Jwt)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<NodeOptions>()
                .BindConfiguration(NodeOptions.Node)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<WalletOptions>()
                .BindConfiguration(WalletOptions.Wallet)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<AutoReceiverOptions>()
                .BindConfiguration(AutoReceiverOptions.AutoReceiver)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<AutoLockerOptions>()
                .BindConfiguration(AutoLockerOptions.AutoLocker)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<PlasmaBotOptions>()
                .BindConfiguration(PlasmaBotOptions.PlasmaBot)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new AddressJsonConverter());
                options.SerializerOptions.Converters.Add(new TokenStandardJsonConverter());
                options.SerializerOptions.Converters.Add(new HashJsonConverter());
            });

            // Events
            services.AddScoped<WalletAccountsAdded>();
            services.AddScoped<WalletInitialized>();
            services.AddScoped<WalletLocked>();
            services.AddScoped<WalletUnlocked>();

            // Services
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<INodeService, NodeService>();
            services.AddSingleton<IWalletService, WalletService>();
            services.AddSingleton<IAutoReceiverService, AutoReceiverService>();
            services.AddSingleton<IAutoLockerService, AutoLockerService>();
            services.AddHttpClient<IPlasmaBotService, PlasmaBotService>();
            services.AddHostedService(p => p.GetRequiredService<IWalletService>());
            services.AddHostedService(p => p.GetRequiredService<IAutoReceiverService>());
            services.AddHostedService(p => p.GetRequiredService<IAutoLockerService>());

            // Subscribe
            services.SubscribeAsync<WalletInitialized, IAutoReceiverService>(lifetime: SubscriberLifetime.Singleton);
            services.SubscribeAsync<WalletUnlocked, IAutoReceiverService>(lifetime: SubscriberLifetime.Singleton);
            services.SubscribeAsync<WalletAccountsAdded, IAutoReceiverService>(lifetime: SubscriberLifetime.Singleton);

            // Validation
            services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

            // Exception handlers
            services.AddExceptionHandler<IncorrectPasswordExceptionHandler>();
            services.AddExceptionHandler<NotFoundExceptionHandler>();
            services.AddExceptionHandler<RemoteInvocationExceptionHandler>();
            services.AddExceptionHandler<SocketExceptionHandler>();
            services.AddExceptionHandler<WalletExceptionHandler>();

            services.AddEndpointsApiExplorer();

            return services;
        }

        public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var root = endpoints.MapGroup("/api")
                .AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory)
                .RequireAuthorization();

            root.MapGroup("/auto-receiver").WithTags("AutoReceiver")
                .MapGetAutoReceiverStatusEndpoint();

            root.MapGroup("/users").WithTags("Users")
                .MapAuthenticateUserEndpoint();

            root.MapGroup("/wallet").WithTags("Wallet")
                .MapGetWalletStatusEndpoint()
                .MapGetWalletAccountsEndpoint()
                .MapAddWalletAccountsEndpoint()
                .MapInitializeWalletEndpoint()
                .MapRestoreWalletEndpoint()
                .MapLockWalletEndpoint()
                .MapUnlockWalletEndpoint();

            root.MapGroup("/ledger").WithTags("Ledger")
                .MapGetAccountInfoEndpoint()
                .MapGetReceivedAccountBlocksEndpoint()
                .MapGetUnreceivedAccountBlocksEndpoint()
                .MapGetPlasmaInfoEndpoint()
                .MapGetFusionInfoEndpoint();

            root.MapGroup("/transfer").WithTags("Transfer")
                .MapSendTransferEndpoint()
                .MapReceiveTransferEndpoint();

            root.MapGroup("/plasma").WithTags("Plasma")
                .MapFusePlasmaEndpoint()
                .MapCancelPlasmaEndpoint();

            root.MapGroup("/utilities").WithTags("Utilities")
                .MapFuseBotPlasmaEndpoint()
                .MapValidateAddressEndpoint();

            return root;
        }

        public static IApplicationBuilder InitApi(this IApplicationBuilder app)
        {
            return app;
        }
    }
}
