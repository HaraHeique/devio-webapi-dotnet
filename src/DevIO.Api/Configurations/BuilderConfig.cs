﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace DevIO.Api.Configurations
{
    public static class BuilderConfig
    {
        public static IConfiguration Build(this ConfigurationBuilder builderConfig, IWebHostEnvironment hostingEnvironment)
        {
            var builder = builderConfig
                .SetBasePath(hostingEnvironment.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();

            if (hostingEnvironment.IsProduction())
            {
                builder.AddUserSecrets<Startup>();
            }

            return builder.Build();
        }
    }
}
