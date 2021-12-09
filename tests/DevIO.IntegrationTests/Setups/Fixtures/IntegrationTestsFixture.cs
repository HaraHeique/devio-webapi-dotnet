using DevIO.Api;
using DevIO.IntegrationTests.Setups.Auth;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;

namespace DevIO.IntegrationTests.Setups.Fixtures
{
    public abstract class IntegrationTestsFixture : IClassFixture<ApiWebApplicationFactory<Startup>>
    {
        protected readonly ApiWebApplicationFactory<Startup> Factory;
        protected readonly HttpClient Client;

        public IntegrationTestsFixture(ApiWebApplicationFactory<Startup> factory)
        {
            Factory = factory;
            Client = CreateClient();
        }

        private HttpClient CreateClient()
        {
            return Factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Bypass (skip) authentication/authorization (always true)
                    services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                });
            }).CreateClient();
        }
    }
}
