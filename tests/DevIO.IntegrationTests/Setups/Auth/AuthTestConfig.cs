using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace DevIO.IntegrationTests.Setups.Auth
{
    public static class AuthTestConfig
    {
        public static IServiceCollection AddTestAuthenticationConfig(this IServiceCollection services)
        {
            // Configuração da autorização
            services.AddAuthorization(opts => 
            {
                opts.DefaultPolicy = new AuthorizationPolicyBuilder(AuthConstants.DefaultScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // Configuração da autenticação (registro do authentication handler criado (customizado))
            services.AddAuthentication(AuthConstants.DefaultScheme)
                .AddScheme<AuthenticationSchemeOptions, IntegrationTestsAuthHandler>(AuthConstants.DefaultScheme, opts => { });

            return services;
        }
    }
}
