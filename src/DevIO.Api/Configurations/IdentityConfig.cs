using DevIO.Api.Data;
using DevIO.Api.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevIO.Api.Configurations
{
    public static class IdentityConfig
    {
        public static IServiceCollection AddIdentityConfig(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<IdentityTranslateMessage>()
                .AddDefaultTokenProviders();

            // JWT
            services.AddJwtConfig(configuration);

            return services;
        }

        public static async Task<IApplicationBuilder> SeedSuperUserSystem(this IApplicationBuilder app, IConfiguration configuration)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var provider = scope.ServiceProvider;

            var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = provider.GetRequiredService<ILogger<Startup>>();

            var adminEmail = configuration["AdminUserInfo:Email"];
            var adminPassword = configuration["AdminUserInfo:Password"];
            var adminRoleName = "Admin";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser is null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (!result.Succeeded)
                {
                    logger.LogError("Falha ao criar admin: {@Errors}", result.Errors.Select(e => e.Description));
                    return app;
                }

                await userManager.AddToRoleAsync(adminUser, adminRoleName);
            }

            return app;
        }

        private static IServiceCollection AddJwtConfig(this IServiceCollection services, IConfiguration configuration)
        {
            // Pega a seção do AppSettings.json e configura usando a classe AppSettings criada
            var appSettingsSection = configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // Obtém o AppSettings a partir de uma instância do objeto e faz o enconding em ASCII
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            // Adicionando e configurando a autenticação criada
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = appSettings.ValidoEm,
                    ValidIssuer = appSettings.Emissor
                };
            });

            return services;
        }
    }
}
