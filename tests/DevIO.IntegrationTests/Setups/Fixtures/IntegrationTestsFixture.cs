using DevIO.IntegrationTests.Setups.Auth;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using System.Net.Http;
using System.Threading;
using Xunit;

namespace DevIO.IntegrationTests.Setups.Fixtures
{
    [Collection(nameof(InfraSingleInstanceCollection))]
    public abstract class IntegrationTestsFixture //: IClassFixture<ApiWebApplicationFactory> (para cada instancia da class que a herda)
    {
        protected readonly ApiWebApplicationFactory Factory;
        protected readonly HttpClient Client;

        public IntegrationTestsFixture(ApiWebApplicationFactory factory)
        {
            Factory = factory;
            Client = CreateClient();
            ReseedDatabase();

            WaitFor(1); // Existe por conta do ReseedDb acima ser assíncrono
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

        private async void ReseedDatabase()
        {
            using var dbConnection = new NpgsqlConnection(Factory.ConnectionString);
            await dbConnection.OpenAsync();
            
            var respawner = await Respawner.CreateAsync(dbConnection, new RespawnerOptions
            {
                SchemasToInclude = ["public"],
                TablesToIgnore = ["__EFMigrationsHistory"],
                WithReseed = true,
                DbAdapter = DbAdapter.Postgres
            });

            await respawner.ResetAsync(dbConnection);
        }

        private static void WaitFor(double seconds) => Thread.Sleep((int)seconds * 1000);
    }
}
