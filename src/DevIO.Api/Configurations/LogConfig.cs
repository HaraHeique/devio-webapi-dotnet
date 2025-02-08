using DevIO.Api.Extensions;
using Elmah.Io.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DevIO.Api.Configurations
{
    public static class LogConfig
    {
        public static IServiceCollection AddLogConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddElmahIo(o =>
            {
                o.ApiKey = "106ccce4cacb47fea92e9267c458433b";
                o.LogId = new Guid("937f4a93-4737-4b27-bf31-241d46bb60de");
            });

            // Para obter outros tipo de logs e não somente log de erros
            //services.AddLogging(builder => 
            //{
            //    builder.AddElmahIo(o => 
            //    {
            //        o.ApiKey = "106ccce4cacb47fea92e9267c458433b";
            //        o.LogId = new Guid("937f4a93-4737-4b27-bf31-241d46bb60de");
            //    });

            //    builder.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
            //});

            // Envia as informações do healthcheck para o elmaio (erros)
            services.AddHealthChecks()
                .AddElmahIoPublisher(opt =>
                {
                    opt.ApiKey = "106ccce4cacb47fea92e9267c458433b";
                    opt.LogId = new Guid("937f4a93-4737-4b27-bf31-241d46bb60de");
                    opt.HeartbeatId = "WEB API Fornecedores";
                })
                .AddCheck("Produtos", new SqlServerHealthCheck(configuration.GetConnectionString("DefaultConnection")))
                .AddSqlServer(configuration.GetConnectionString("DefaultConnection"), name: "BancoSQL");

            return services;
        }

        public static IApplicationBuilder UseLogConfig(this IApplicationBuilder app)
        {
            app.UseElmahIo();

            return app;
        }
    }
}
