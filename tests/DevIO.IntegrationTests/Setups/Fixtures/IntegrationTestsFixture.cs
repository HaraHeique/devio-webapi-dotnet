using DevIO.Api;
using DevIO.IntegrationTests.Setups.Auth;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using Xunit;

namespace DevIO.IntegrationTests.Setups.Fixtures
{
    public abstract class IntegrationTestsFixture : IDisposable, IClassFixture<ApiWebApplicationFactory<Startup>>
    {
        protected readonly ApiWebApplicationFactory<Startup> Factory;
        protected readonly HttpClient Client;

        public IntegrationTestsFixture(ApiWebApplicationFactory<Startup> factory)
        {
            Factory = factory;
            Client = CreateClient();
            ConfigureReesedDb();
        }

        public void Dispose()
        {
            // TODO Colocar as liberações de recursos aqui
        }

        protected HttpClient CreateClient(AuthUserTest authUser = null)
        {
            return Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    if (authUser == null)
                    {
                        services.AddSingleton<IPolicyEvaluator, BypassPolicyEvaluator>();
                    }
                    else
                    {
                        services.AddTestAuthenticationConfig(); // Custom handler criado
                        services.AddScoped(_ => authUser); // Mock de usuário de teste injetado por DI
                    }
                });
            }).CreateClient();
        }

        private void ConfigureReesedDb()
        {
            var checkpoint = new Checkpoint
            {
                SchemasToInclude = new string[] { "dbo" },
                TablesToIgnore = new string[] { "__EFMigrationsHistory" },
                WithReseed = true
            };

            checkpoint.Reset(Factory.Configuration.GetConnectionString("DefaultConnection")).Wait();
        }
    }
}
