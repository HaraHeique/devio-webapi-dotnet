using Asp.Versioning.ApiExplorer;
using DevIO.Api.Configurations;
using DevIO.Api.Extensions;
using DevIO.Data.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevIO.Api;

public static class Startup
{
    private const string ConnectionStringKey = "DefaultConnection";

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDataContext>(opt => opt.UseSqlServer(configuration.GetConnectionString(ConnectionStringKey)));

        services.AddIdentityConfig(configuration);

        services.AddAutoMapper(typeof(Program));

        services.AddWebApiConfig();

        // TODO: Mudar aqui para Serilog depois, pois usa o Elmah IO que é pago
        //services.AddLogConfig(configuration);

        services.ResolveDependencies();

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
    }
}
