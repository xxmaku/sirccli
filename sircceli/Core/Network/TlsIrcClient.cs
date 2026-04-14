using sircceli.Configuration;
using sircceli.Models;

namespace sircceli.Core.Network;

public class TlsIrcClient : IIrcClient, IDisposable
{
    public TlsIrcClient(IrcClientConfiguration cfg)
        => throw new NotImplementedException();

    public bool IsConnected { get; }
    public IReadOnlyList<string> CurrentUsers { get; }
    public event EventHandler<bool>? ConnectionStateChanged;
    public event EventHandler<IReadOnlyList<string>>? ChannelUsersChanged;
    public event EventHandler<Message>? MessageReceived;

    void IIrcClient.Dispose()
    {
        throw new NotImplementedException();
    }

    public Task SendMessage(string message)
    {
        throw new NotImplementedException();
    }

    void IDisposable.Dispose()
    {
        throw new NotImplementedException();
    }
}