using DevIO.Api.Data;
using DevIO.Data.Context;
using Docker.DotNet.Models;
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
    public class ApiWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class, IAsyncLifetime
    {
        private readonly MsSqlContainer _sqlServerContainer;

        public IConfiguration Configuration { get; private set; }
        public IWebHostEnvironment Env { get; private set; }

        public ApiWebApplicationFactory()
        {
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
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                Configuration = config.Build();
                Env = context.HostingEnvironment;
            });

            builder.ConfigureTestServices(services => 
            {
                var descriptors = services.Where(s => s.ServiceType == typeof(DbContextOptions<AppDataContext>) || s.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                foreach (var item in descriptors) services.Remove(item);

                services.AddDbContext<AppDataContext>(options => options.UseSqlServer(_sqlServerContainer.GetConnectionString()));
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(_sqlServerContainer.GetConnectionString()));
            });
        }

        public async Task InitializeAsync()
        {
            await _sqlServerContainer.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _sqlServerContainer.StopAsync();
        }
    }
}
