using Microsoft.Extensions.DependencyInjection;
using sircceli.Core;
using sircceli.Core.Network;

namespace sircceli.Configuration;

public static class DependencyInjections
{
    public static void AddSircceli(this IServiceCollection services)
    {
        services.AddTransient<TcpIrcClient>();
        services.AddTransient<TlsIrcClient>();
        services.AddTransient<IrcClientFactory>();
    }
}