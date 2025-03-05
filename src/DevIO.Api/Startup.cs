using DevIO.Api.Configurations;
using DevIO.Api.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevIO.Api;

public class Startup
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        IServiceCollection services = builder.Services;
        IConfiguration configuration = builder.Configuration;
        IWebHostEnvironment env = builder.Environment;

        builder.Configure();

        services.AddIdentityConfig(configuration, env);

        services.AddAutoMapper(typeof(Program));

        services.AddWebApiConfig();

        // TODO: Mudar aqui para Serilog depois, pois usa o Elmah IO que é pago
        //services.AddLogConfig(configuration);

        services.ResolveDependencies();

        services.RegisterContextDependencies(configuration, env);

        services.AddOpenApiConfig();
    }

    public static void Configure(WebApplication app)
    {
        IWebHostEnvironment env = app.Environment;

        if (env.IsDevelopment())
        {
            app.UseCors("Development");
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseCors("Production");
            app.UseHsts();
            app.UseMiddleware<ExceptionMiddleware>();

            // TODO: Mudar aqui para Serilog depois, pois usa o Elmah IO que é pago
            //app.UseLogConfig();
        }

        app.UseGlobalizationConfig();

        app.UseOpenApiConfig();

        app.UseWebApiConfig();

        app.RunMigrations();
    }
}
