using sircceli.Models;

namespace sircceli.Core.Network;

public interface IIrcClient : IDisposable
{
    bool IsConnected { get; }
    IReadOnlyList<string> CurrentUsers { get; }
    event EventHandler<bool>? ConnectionStateChanged;
    event EventHandler<IReadOnlyList<string>>? ChannelUsersChanged;
    event EventHandler<string>? ChannelJoined;
    event EventHandler<string>? NickChanged;
    event EventHandler<Message>? MessageReceived;
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task DisconnectAsync();
    Task SendRawMessage(string message);
    Task SendMessage(string target, string message);
}
