using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Numerics;
using System.Text;
using Zenon;
using Zenon.Model.Embedded.Json;
using Zenon.Model.NoM;
using Zenon.Model.NoM.Json;
using Zenon.Model.Primitives;
using Zenon.Utils;
using ZenonWalletApi;
using ZenonWalletApi.Authorization;
using ZenonWalletApi.Models;
using ZenonWalletApi.Models.Converters;
using ZenonWalletApi.Models.Exceptions;
using ZenonWalletApi.Services;
using ZenonWalletApi.Services.ExceptionHandlers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IPlasmaBotService, PlasmaBotService>();

// Configuration
builder.Services.AddOptions<ApiOptions>()
    .BindConfiguration(ApiOptions.Api)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.Jwt)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<NodeOptions>()
    .BindConfiguration(NodeOptions.Node)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<WalletOptions>()
    .BindConfiguration(WalletOptions.Wallet)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<AutoReceiveOptions>()
    .BindConfiguration(AutoReceiveOptions.AutoReceiver)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<AutoLockServiceOptions>()
    .BindConfiguration(AutoLockServiceOptions.AutoLocker)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<PlasmaBotOptions>()
    .BindConfiguration(PlasmaBotOptions.PlasmaBot)
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<INodeService, NodeService>();
builder.Services.AddSingleton<IWalletService, WalletService>();
builder.Services.AddSingleton<IAutoReceiveService, AutoReceiveService>();
builder.Services.AddSingleton<IAutoLockService, AutoLockService>();

builder.Services.AddHostedService(p => p.GetRequiredService<IWalletService>());
builder.Services.AddHostedService(p => p.GetRequiredService<IAutoReceiveService>());
builder.Services.AddHostedService(p => p.GetRequiredService<IAutoLockService>());

builder.Services.AddSingleton<IAuthorizationHandler, UserRoleRequirementAuthorizationHandler>();

// Validation
builder.Services.AddValidatorsFromAssemblyContaining<Program>(ServiceLifetime.Singleton);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new AddressJsonConverter());
    options.SerializerOptions.Converters.Add(new TokenStandardJsonConverter());
    options.SerializerOptions.Converters.Add(new HashJsonConverter());
});
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<Address>(() => new OpenApiSchema { Type = "string" });
    options.MapType<TokenStandard>(() => new OpenApiSchema { Type = "string" });
    options.MapType<Hash>(() => new OpenApiSchema { Type = "string" });

    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Zenon Wallet API",
        Version = "v1",
        Description = "Wallet API for Zenon Network",
        Contact = new OpenApiContact()
        {
            Name = "Zenon Network",
            Url = new Uri("https://zenon.network")
        },
        License = new OpenApiLicense()
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new string[] {}
        }
    });
});

if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
        options.HttpsPort = 443;
    });
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var authOptions = builder.Configuration
        .GetSection(JwtOptions.Jwt)
        .Get<JwtOptions>()!;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Secret)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = authOptions.ValidAudience,
        ValidIssuer = authOptions.ValidIssuer,
        ValidateLifetime = authOptions.ExpiresOn.HasValue || authOptions.ExpiresAfter.HasValue,
        // set clock skew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new UserRoleRequirement(UserRoles.Admin));
    });
    options.AddPolicy("User", policy =>
    {
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new UserRoleRequirement(UserRoles.User));
    });
});
builder.Services.AddEndpointsApiExplorer();

// Exception handlers
builder.Services.AddExceptionHandler<IncorrectPasswordExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<RemoteInvocationExceptionHandler>();
builder.Services.AddExceptionHandler<SocketExceptionHandler>();
builder.Services.AddExceptionHandler<WalletExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(async statusCodeContext
    => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
                 .ExecuteAsync(statusCodeContext.HttpContext));

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zenon Wallet API V1");
});

var root = app.MapGroup("/api");
root.AddEndpointFilterFactory(ValidationFilter.ValidationFilterFactory);
root.RequireAuthorization();

#region Users Endpoints

var usersGroup = root.MapGroup("/users")
    .WithTags("users");

usersGroup.MapPost("/authenticate", async (
    [FromServices] IUserService users,
    [Validate] AuthenticateRequest request) =>
{
    var result = await users.AuthenticateAsync(request);

    if (result == null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(result!);
})
    .Produces<AuthenticateResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .ProducesValidationProblem()
    .AllowAnonymous();

#endregion

#region Wallet Endpoints

var walletGroup = root.MapGroup("/wallet")
    .WithTags("wallet");

walletGroup.MapGet("/status", (
    [FromServices] IWalletService service) =>
{
    return new WalletStatusResponse(service.IsInitialized, service.IsUnlocked);
})
    .Produces<WalletStatusResponse>()
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .RequireAuthorization("User");

walletGroup.MapPost("/{accountIndex}/address", async (
    [FromServices] IWalletService wallet,
    [Validate] AccountIndex accountIndex) =>
{
    var address = await wallet.GetAccountAddressAsync(accountIndex.value);

    return new WalletAccountAddressResponse(address);
})
    .Produces<WalletAccountAddressResponse>()
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem()
    .RequireAuthorization("User");

walletGroup.MapPost("/init", async (
    [FromServices] IWalletService service,
    [Validate] InitWalletRequest request) =>
{
    var mnemonic = await service.InitAsync(request.password);

    return new InitWalletResponse(mnemonic);
})
    .Produces<InitWalletResponse>()
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem()
    .RequireAuthorization("Admin");

walletGroup.MapPost("/restore", async (
    [FromServices] IWalletService service,
    [Validate] RestoreWalletRequest request) =>
{
    await service.RestoreAsync(request.password, request.mnemonic);

    return Results.Ok();
})
    .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem()
    .RequireAuthorization("Admin");

walletGroup.MapPost("/unlock", async (
    [FromServices] IWalletService service,
    [Validate] UnlockWalletRequest request) =>
{
    await service.UnlockAsync(request.password);

    return Results.Ok();
})
    .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem()
    .RequireAuthorization("User");

walletGroup.MapPost("/lock", async (
    [FromServices] IWalletService service) =>
{
    await service.LockAsync();

    return Results.Ok();
})
    .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .RequireAuthorization("User");

#endregion

#region Ledger Endpoints

var ledgerGroup = root.MapGroup("/ledger")
    .WithTags("ledger")
    .RequireAuthorization("User");

ledgerGroup.MapGet("/{address}/balances", async (
    [FromServices] INodeService client,
    [Validate] AddressString address) =>
{
    await client.ConnectAsync();

    var result = await client.Api.Ledger.GetAccountInfoByAddress(address.value);

    return result.ToJson();
})
    .Produces<JAccountInfo>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesValidationProblem();

ledgerGroup.MapGet("/{address}/received", async (
    [FromServices] IWalletService wallet,
    [FromServices] INodeService client,
    [Validate] AddressString address,
    [AsParameters][Validate] TransferReceivedRequest request) =>
{
    await client.ConnectAsync();

    // Retrieve all received account blocks by address
    var result = await client.Api.Ledger
        .GetAccountBlocksByPage(address.value,
            pageIndex: (uint)request.pageIndex,
            pageSize: (uint)request.pageSize);

    return result.ToJson();
})
    .Produces<JAccountBlockList>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesValidationProblem();

ledgerGroup.MapGet("/{address}/unreceived", async (
    [FromServices] IWalletService wallet,
    [FromServices] INodeService client,
    [Validate] AddressString address,
    [AsParameters][Validate] TransferUnreceivedRequest request) =>
{
    await client.ConnectAsync();

    // Retrieve all unreceived account blocks by address
    var result = await client.Api.Ledger
        .GetUnreceivedBlocksByAddress(address.value,
            pageIndex: (uint)request.pageIndex,
            pageSize: (uint)request.pageSize);

    return result.ToJson();
})
    .Produces<JAccountBlockList>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesValidationProblem();

ledgerGroup.MapGet("{address}/plasma", async (
    [FromServices] INodeService client,
    [Validate] AddressString address) =>
{
    await client.ConnectAsync();

    // Retrieve plasma info
    var result = await client.Api.Embedded.Plasma.Get(address.value);

    return result.ToJson();
})
    .Produces<JPlasmaInfo>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesValidationProblem();

ledgerGroup.MapGet("/{address}/fused", async (
    [FromServices] IWalletService wallet,
    [FromServices] INodeService client,
    [Validate] AddressString address,
    [AsParameters][Validate] PlasmaFusedRequest request) =>
{
    await client.ConnectAsync();

    var syncInfo = await client.Api.Stats.SyncInfo();

    // Retrieve plasma info
    var result = await client.Api.Embedded.Plasma
        .GetEntriesByAddress(address.value, (uint)request.pageIndex, (uint)request.pageSize);

    var json = result.ToJson();

    foreach (var element in json.list)
    {
        element.isRevocable = syncInfo.currentHeight > element.expirationHeight;
    }

    return json;
})
    .Produces<JFusionEntryList>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesValidationProblem();

#endregion

#region Transfer Endpoints

var transferGroup = root.MapGroup("/transfer")
    .WithTags("transfer")
    .RequireAuthorization("User");

transferGroup.MapPost("/{accountIndex}/send", async (
    [FromServices] IWalletService wallet,
    [FromServices] INodeService client,
    [Validate] AccountIndex accountIndex,
    [FromBody][Validate] SendTransferRequest request) =>
{
    await client.ConnectAsync();

    // Access wallet account
    var account = await wallet.GetAccountAsync(accountIndex.value);

    // Get wallet account address
    var address = await account.GetAddressAsync();

    BigInteger amount;
    if (request.tokenStandard == TokenStandard.ZnnZts ||
        request.tokenStandard == TokenStandard.QsrZts)
    {
        amount = AmountUtils.ExtractDecimals(request.amount, Constants.CoinDecimals);
    }
    else
    {
        var token = await client.Api.Embedded.Token.GetByZts(request.tokenStandard);
        if (token == null)
        {
            throw new NotFoundException("Token does not exist");
        }
        amount = AmountUtils.ExtractDecimals(request.amount, (int)token.Decimals);
    }

    // Retrieve account info
    var accountInfo = await client.Api.Ledger
        .GetAccountInfoByAddress(address);

    // Find balance info
    var balanceInfo = accountInfo.BalanceInfoList
        .FirstOrDefault(x => x.Token.TokenStandard == request.tokenStandard);

    // Check balance
    if (balanceInfo == null)
    {
        throw new NotFoundException($"You do not have any {request.tokenStandard} tokens");
    }
    else if (balanceInfo.Balance < amount)
    {
        if (balanceInfo.Balance == BigInteger.Zero)
        {
            throw new NotFoundException($"You do not have any {balanceInfo.Token.Symbol} tokens");
        }
        else
        {
            throw new NotFoundException($"You do not have enough {balanceInfo.Token.Symbol} tokens");
        }
    }

    // Create send block
    var block = AccountBlockTemplate.Send(
        client.ProtocolVersion, client.ChainIdentifier,
        request.address, request.tokenStandard, amount);

    // Send block
    var response = await client.SendAsync(block, account);

    // Return block hash
    return response.ToJson();
})
    .Produces<JAccountBlockTemplate>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem();

transferGroup.MapPost("/{accountIndex}/receive", async (
    [FromServices] IWalletService wallet,
    [FromServices] INodeService client,
    [Validate] AccountIndex accountIndex,
    [FromBody][Validate] ReceiveTransferRequest request) =>
{
    await client.ConnectAsync();

    // Access wallet account from index
    var account = await wallet.GetAccountAsync(accountIndex.value);

    // Create receive block
    var block = AccountBlockTemplate.Receive(
        client.ProtocolVersion, client.ChainIdentifier,
        request.blockHash);

    // Send block
    var response = await client.SendAsync(block, account);

    return response.ToJson();
})
    .Produces<JAccountBlockTemplate>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem();

#endregion

#region Plasma Endpoints

var plasmaGroup = root.MapGroup("/plasma")
    .WithTags("plasma")
    .RequireAuthorization("User");

plasmaGroup.MapPost("/{accountIndex}/fuse", async (
    [FromServices] IWalletService wallet,
    [FromServices] INodeService client,
    [Validate] AccountIndex accountIndex,
    [FromBody][Validate] FusePlasmaRequest request) =>
{
    await client.ConnectAsync();

    // Access wallet account from index
    var account = await wallet.GetAccountAsync(accountIndex.value);

    // Retrieve wallet account address
    var address = await account.GetAddressAsync();

    var amount = AmountUtils.ExtractDecimals(request.amount, Constants.CoinDecimals);

    // Create block
    var block = client.Api.Embedded.Plasma.Fuse(request.address, amount);

    // Send block
    var response = await client.SendAsync(block, account);

    return response.ToJson();
})
    .Produces<JAccountBlockTemplate>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem();

plasmaGroup.MapPost("/{accountIndex}/cancel", async (
    [FromServices] IWalletService wallet,
    [FromServices] INodeService client,
    [Validate] AccountIndex accountIndex,
    [FromBody][Validate] CancelPlasmaRequest request) =>
{
    await client.ConnectAsync();

    // Access wallet account from index
    var account = await wallet.GetAccountAsync(accountIndex.value);

    // Retrieve wallet account address
    var address = await account.GetAddressAsync();

    // Create block
    var block = client.Api.Embedded.Plasma.Cancel(request.idHash);

    // Send block
    var response = await client.SendAsync(block, account);

    return response.ToJson();
})
    .Produces<JAccountBlockTemplate>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem();

#endregion

#region AutoReceive

var autoreceiveGroup = root.MapGroup("/autoreceiver")
    .WithTags("autoreceiver")
    .RequireAuthorization("User");

autoreceiveGroup.MapPut("/{accountIndex}", async (
    [FromServices] IAutoReceiveService autoReceiver,
    [Validate] AccountIndex accountIndex) =>
{
    await autoReceiver.SubscribeAsync(accountIndex.value);

    return Results.Ok();
})
    .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem();

autoreceiveGroup.MapDelete("/{accountIndex}", async (
    [FromServices] IAutoReceiveService autoReceiver,
    [Validate] AccountIndex accountIndex) =>
{
    await autoReceiver.UnsubscribeAsync(accountIndex.value);

    return Results.Ok();
})
    .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status409Conflict)
    .ProducesValidationProblem();

#endregion

#region Utilities

var utilsGroup = root.MapGroup("/utilities")
    .WithTags("utilities")
    .RequireAuthorization("User");

utilsGroup.MapPost("/plasma-bot/fuse", async (
    [FromServices] IPlasmaBotService plasmaBot,
    [FromBody][Validate] FuseBotPlasmaRequest request) =>
{
    return await plasmaBot.FuseAsync(request.address);
})
    .Produces(StatusCodes.Status200OK, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status401Unauthorized, typeof(string), contentType: "text/plain")
    .Produces(StatusCodes.Status403Forbidden, typeof(string), contentType: "text/plain")
    .ProducesProblem(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
    .ProducesValidationProblem();

#endregion

app.Run();