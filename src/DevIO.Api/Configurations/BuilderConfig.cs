using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DevIO.Api.Configurations
{
    public static class BuilderConfig
    {
        public static void Configure(this WebApplicationBuilder appBuilder)
        {
            var env = appBuilder.Environment;

            var builder = appBuilder.Configuration
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            if (env.IsProduction())
            {
                builder.AddUserSecrets<Program>();
            }
        }
    }
}
