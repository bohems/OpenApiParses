using Microsoft.Extensions.DependencyInjection;

namespace SwaggerRequests;

public static class DependencyInjection
{
    public static IServiceCollection AddOpenApiRequests(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.AddScoped<OpenApiRequestService>();
        
        return services;
    }
}