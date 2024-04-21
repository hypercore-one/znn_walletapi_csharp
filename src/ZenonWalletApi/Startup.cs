using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ZenonWalletApi.Infrastructure.Authorization;
using ZenonWalletApi.Infrastructure.Configurations;
using ZenonWalletApi.Infrastructure.Registrations;
using ZenonWalletApi.Models;
using ZenonWalletApi.Options;

namespace ZenonWalletApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.AddSwagger(_configuration);

            services.AddApiServices(_configuration);

            services.AddProblemDetails();

            // Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var authOptions = _configuration
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

            // Authorization
            services.AddAuthorization(options =>
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

            services.AddSingleton<IAuthorizationHandler, UserRoleRequirementAuthorizationHandler>();
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStatusCodePages();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseRouting();

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapApiEndpoints();
            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zenon Wallet API V1");
            });

            app.InitApi();
        }
    }
}
