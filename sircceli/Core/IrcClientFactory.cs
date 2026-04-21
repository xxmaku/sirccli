using sircceli.Configuration;
using sircceli.Core.Network;

namespace sircceli.Core;

public static class IrcClientFactory
{
    public static IIrcClient Create(IrcClientConfiguration cfg)
        => cfg.UseTls ? CreateTlsClient(cfg) : CreateTcpClient(cfg);
    private static TcpIrcClient CreateTcpClient(IrcClientConfiguration cfg) => new(cfg);
    private static TlsIrcClient CreateTlsClient(IrcClientConfiguration cfg) => new(cfg);
}
