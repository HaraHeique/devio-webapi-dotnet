using DevIO.Api.Data;
using DevIO.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace DevIO.Api.Configurations
{
    public static class DatabaseMigratorConfig
    {
        public static IServiceCollection RunMigrations(this IServiceCollection services)
        {
            using var provider = services.BuildServiceProvider();

            var dataContext = provider.GetRequiredService<AppDataContext>();
            var identityContext = provider.GetRequiredService<ApplicationDbContext>();

            ExecuteMigrations(dataContext);
            ExecuteMigrations(identityContext);

            return services;
        }

        private static void ExecuteMigrations(DbContext context)
        {
            if (context.Database.GetPendingMigrations().Any())
                context.Database.Migrate(); // Esta é a linha mais importante e de fato executa as migrations
        }
    }
}
