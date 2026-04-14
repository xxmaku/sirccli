using sircceli.Models;

namespace sircceli.Core.Network;

public interface IIrcClient
{
    bool IsConnected { get; }
    IReadOnlyList<string> CurrentUsers { get; }
    event EventHandler<bool>? ConnectionStateChanged;
    event EventHandler<IReadOnlyList<string>>? ChannelUsersChanged;
    event EventHandler<Message>? MessageReceived;
    void Dispose();
    Task SendMessage(string message);
}