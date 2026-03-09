using Microsoft.Extensions.DependencyInjection;

namespace sircceli.Configuration;

public static class DependencyInjections
{
    public static IServiceCollection AddSircceli(this IServiceCollection services)
    {
        return services;
    }
}