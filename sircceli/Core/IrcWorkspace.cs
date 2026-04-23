using sircceli.Configuration;
using sircceli.Models;

namespace sircceli.Core;

public sealed class IrcWorkspace
{
    private readonly Dictionary<string, ChannelBuffer> _channels = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Message> _statusMessages = new();
    private readonly object _gate = new();

    public IrcWorkspace()
        : this(IrcClientConfiguration.LoadFromAppSettings().Channel)
    {
    }

    public IrcWorkspace(string initialChannel)
    {
        SetActiveChannel(initialChannel);
    }

    public event EventHandler? Changed;

    public ChannelBuffer ActiveChannel
    {
        get
        {
            lock (_gate)
            {
                return _channels[ActiveChannelName];
            }
        }
    }

    public string ActiveChannelName { get; private set; } = string.Empty;

    public IReadOnlyList<ChannelBuffer> GetChannels()
    {
        lock (_gate)
        {
            return _channels.Values.OrderBy(channel => channel.Name).ToArray();
        }
    }

    public IReadOnlyList<Message> StatusMessages
    {
        get
        {
            lock (_gate)
            {
                return _statusMessages.ToArray();
            }
        }
    }

    public ChannelBuffer EnsureChannel(string channelName)
    {
        channelName = NormalizeChannel(channelName);
        lock (_gate)
        {
            if (_channels.TryGetValue(channelName, out var existing))
                return existing;

            var channel = new ChannelBuffer(channelName);
            _channels[channelName] = channel;
            return channel;
        }
    }

    public void SetActiveChannel(string channelName)
    {
        var channel = EnsureChannel(channelName);
        lock (_gate)
        {
            if (ActiveChannelName.Equals(channel.Name, StringComparison.OrdinalIgnoreCase))
                return;

            ActiveChannelName = channel.Name;
            channel.MarkRead();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveChannel(string channelName)
    {
        channelName = NormalizeChannel(channelName);
        var removed = false;
        lock (_gate)
        {
            if (_channels.Count == 1)
                return;

            removed = _channels.Remove(channelName);
            if (!ActiveChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase))
            {
                if (!removed)
                    return;

                Changed?.Invoke(this, EventArgs.Empty);
                return;
            }

            ActiveChannelName = _channels.Keys.OrderBy(name => name).First();
            _channels[ActiveChannelName].MarkRead();
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void AddMessage(string channelName, Message message)
    {
        var channel = EnsureChannel(channelName);
        var isActive = channel.Name.Equals(ActiveChannelName, StringComparison.OrdinalIgnoreCase);
        channel.AddMessage(message, isActive);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void AddStatusMessage(Message message)
    {
        lock (_gate)
        {
            _statusMessages.Add(message);
            if (_statusMessages.Count > 200)
                _statusMessages.RemoveAt(0);
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void SetUsers(string channelName, IReadOnlyList<string> users)
    {
        EnsureChannel(channelName).SetUsers(users);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static string NormalizeChannel(string channel)
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw new ArgumentException("Channel cannot be empty.", nameof(channel));

        channel = channel.Trim();
        return channel.StartsWith('#') ? channel : $"#{channel}";
    }
}
