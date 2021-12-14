using DevIO.Api;
using DevIO.IntegrationTests.Setups.Auth;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System;
using System.Net.Http;
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

        private HttpClient CreateClient()
        {
            return Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IPolicyEvaluator, BypassPolicyEvaluator>();
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
