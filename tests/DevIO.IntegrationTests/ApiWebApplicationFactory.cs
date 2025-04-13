using DevIO.Api;
using DevIO.Api.Configurations;
using DevIO.Api.Data;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;
using Xunit;

namespace DevIO.IntegrationTests
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MsSqlContainer _sqlServerContainer;
        private readonly PostgreSqlContainer _postgreSqlContainer;

        private bool _migrationAlreadyApplied = false;

        public const string EnvironmentName = "Testing";

        public string ConnectionString => _postgreSqlContainer.GetConnectionString();
        public IConfiguration Configuration { get; private set; }
        public IWebHostEnvironment Env { get; private set; }

        public ApiWebApplicationFactory()
        {
            // TODO: MUDAR AQUI DE SQL SERVER PARA POSTEGRESQL
            // Para mais infos aqui: https://www.milanjovanovic.tech/blog/testcontainers-integration-testing-using-docker-in-dotnet
            //_sqlServerContainer = new MsSqlBuilder()
            //    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            //    .WithName("app-web-api-completa-database")
            //    .WithPassword("Admin@123")
            //    .WithPortBinding(1433, 1433)
            //    .WithEnvironment("ACCEPT_EULA", "Y")
            //    .WithCleanUp(true)
            //    .Build();

            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:latest")
                .WithDatabase("DevIOWebApiDotNetTests")
                .WithUsername("postgres-tests")
                .WithPassword("postgres-tests")
                .WithCleanUp(true)
                .Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment(EnvironmentName);

            builder.ConfigureAppConfiguration((context, config) =>
            {
                Configuration = config.Build();
                Env = context.HostingEnvironment;
            });

            builder.ConfigureTestServices(services =>
            {
                var descriptors = services
                    .Where(s => s.ServiceType == typeof(DbContextOptions<AppDataContext>) || s.ServiceType == typeof(DbContextOptions<ApplicationDbContext>))
                    .ToList();

                foreach (var item in descriptors) services.Remove(item);

                services
                    .AddDbContext<AppDataContext>(options => options.UseNpgsql(_postgreSqlContainer.GetConnectionString()))
                    .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(_postgreSqlContainer.GetConnectionString()));
                
                RunMigrationsOnce(services);
            });
        }

        private void RunMigrationsOnce(IServiceCollection services)
        {
            if (_migrationAlreadyApplied) return;

            services.RunMigrations();

            _migrationAlreadyApplied = true;
        }

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();

            // Posso também ler um script da base e montar a base dela
            //var migrationSql = await System.IO.File.ReadAllTextAsync("migration_aqui.sql");
            //await _sqlServerContainer.ExecScriptAsync(migrationSql);
        }

        public async new Task DisposeAsync()
        {
            await _postgreSqlContainer.StopAsync();
        }
    }
}
