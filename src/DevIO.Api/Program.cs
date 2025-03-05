using Microsoft.AspNetCore.Builder;

namespace DevIO.Api;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        Startup.ConfigureServices(builder);

        WebApplication app = builder.Build();

        Startup.Configure(app);

        app.Run();
    }
}
