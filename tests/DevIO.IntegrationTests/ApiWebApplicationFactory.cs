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
using Xunit;

namespace DevIO.IntegrationTests
{
    public class ApiWebApplicationFactory : WebApplicationFactory<Startup>, IAsyncLifetime
    {
        private readonly MsSqlContainer _sqlServerContainer;

        public const string EnvironmentName = "Testing";

        public string ConnectionString => _sqlServerContainer.GetConnectionString();
        public IConfiguration Configuration { get; private set; }
        public IWebHostEnvironment Env { get; private set; }

        public ApiWebApplicationFactory()
        {
            // Para mais infos aqui: https://www.milanjovanovic.tech/blog/testcontainers-integration-testing-using-docker-in-dotnet
            _sqlServerContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithName("app-web-api-completa-database")
                .WithPassword("Admin@123")
                .WithPortBinding(1433, 1433)
                .WithEnvironment("ACCEPT_EULA", "Y")
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
                    .AddDbContext<AppDataContext>(options => options.UseSqlServer(_sqlServerContainer.GetConnectionString()))
                    .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(_sqlServerContainer.GetConnectionString()));

                services.RunMigrations();
            });
        }

        public async Task InitializeAsync()
        {
            await _sqlServerContainer.StartAsync();

            // Posso também ler um script da base e montar a base dela
            //var migrationSql = await System.IO.File.ReadAllTextAsync("migration_aqui.sql");
            //await _sqlServerContainer.ExecScriptAsync(migrationSql);
        }

        public async new Task DisposeAsync()
        {
            await _sqlServerContainer.StopAsync();
        }
    }
}
