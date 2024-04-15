using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Infrastructure.Registrations
{
    public static class SwaggerRegistration
    {
        public static void AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(options =>
            {
                var fileName = typeof(SwaggerRegistration).Assembly.GetName().Name;
                var filePath = Path.Combine(AppContext.BaseDirectory, $"{fileName}.xml");
                options.IncludeXmlComments(filePath);

                options.MapType<Address>(() => new OpenApiSchema { Type = "string" });
                options.MapType<TokenStandard>(() => new OpenApiSchema { Type = "string" });
                options.MapType<Hash>(() => new OpenApiSchema { Type = "string" });

                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Version = "v1",
                    Title = "Zenon Wallet API",
                    Description = "A .NET based cross-platform Wallet API for interacting with Zenon Alphanet - Network of Momentum Phase 1",
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
        }
    }
}
