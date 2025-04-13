using DevIO.Api.Data;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;

namespace DevIO.Api.Configurations
{
    public static class DatabaseSetupConfig
    {
        private const string ConnectionStringKey = "DefaultConnection";

        public static IServiceCollection RegisterDbContextDependencies(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            string connString = configuration.GetConnectionString(ConnectionStringKey);

            services.AddDbContext<ApplicationDbContext>(opt => opt.UseNpgsql(connString));
            services.AddDbContext<AppDataContext>(opt => opt.UseNpgsql(connString));

            return services;
        }
    }

    /// <summary>
    /// Generate migrations before running this method, you can use the following command:
    /// Nuget package manager: Add-Migration {desired_migration_name} -context {desired_context_class_name}
    /// Dotnet CLI: dotnet ef migrations add {desired_migration_name} -c {desired_context_class_name}
    /// Alternatives are:
    /// - Background Service specific for this without stoping application running
    /// - Another project with physical deployment running independently
    /// - Using CI/CD Pipeline in Azure DevOps, Github Actions and so on
    /// </summary>
    public static class DatabaseMigratorConfig
    {
        public static IApplicationBuilder RunMigrations(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            ExecuteMigrations(scope.ServiceProvider);

            return app;
        }

        public static IServiceCollection RunMigrations(this IServiceCollection services)
        {
            using var provider = services.BuildServiceProvider();

            ExecuteMigrations(provider);

            return services;
        }

        private static void ExecuteMigrations(IServiceProvider provider)
        {
            var dataContext = provider.GetRequiredService<AppDataContext>();
            var identityContext = provider.GetRequiredService<ApplicationDbContext>();
            var logger = provider.GetRequiredService<ILogger<Startup>>();

            // TODO: Melhorar isto aqui, mas é para esperar para o banco subir no docker-compose localmente
            Thread.Sleep(5000);

            Execute(dataContext, logger);
            Execute(identityContext, logger);

            static void Execute(DbContext context, ILogger logger)
            {
                try
                {
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate(); // Esta é a linha mais importante e de fato executa as migrations
                        logger.LogInformation("Migrations have been applied successfully!");
                    }
                    else
                    {
                        logger.LogInformation("There are no migration to apply");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Errors have occured while trying to apply migrations");
                }
            }
        }
    }
}
