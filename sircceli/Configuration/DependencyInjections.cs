using Microsoft.Extensions.DependencyInjection;
using sircceli.Core.Network;

namespace sircceli.Configuration;

public static class DependencyInjections
{
    public static void AddSircceli(this IServiceCollection services)
    {
        services.AddSingleton<IrcClient>();
    }
}