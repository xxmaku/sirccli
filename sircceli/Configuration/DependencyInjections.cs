using Microsoft.Extensions.DependencyInjection;
using sircceli.Core;
using sircceli.Core.Commands;
using sircceli.Core.Network;

namespace sircceli.Configuration;

public static class DependencyInjections
{
    public static void AddSircceli(this IServiceCollection services)
    {
        services.AddSingleton(new IrcClientConfiguration());
        services.AddTransient<TcpIrcClient>();
        services.AddTransient<TlsIrcClient>();
        services.AddSingleton<IrcWorkspace>(provider =>
            new IrcWorkspace(provider.GetRequiredService<IrcClientConfiguration>().Channel));
        services.AddSingleton<IrcClientSession>();
        services.AddSingleton<IIrcClient>(provider => provider.GetRequiredService<IrcClientSession>());
        services.AddSingleton<IrcInputHandler>();
    }
}
