using Microsoft.Extensions.Hosting;
using SwaggerRequests;

namespace ConsoleApp1;

public static class Program
{
    private static void Main(string[] args)
    {
        var builder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddOpenApiRequests();
            }).UseConsoleLifetime();
 
        var host = builder.Build();
    }
}