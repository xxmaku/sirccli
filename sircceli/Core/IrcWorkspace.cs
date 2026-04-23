using sircceli.Configuration;
using sircceli.Models;

namespace sircceli.Core;

public sealed class IrcWorkspace
{
    private readonly Dictionary<string, ChannelBuffer> _channels = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Message> _statusMessages = new();
    private readonly object _gate = new();
    private long _revision;

    public IrcWorkspace()
        : this(IrcClientConfiguration.LoadFromAppSettings().Channel)
    {
    }

    public IrcWorkspace(string initialChannel)
    {
        SetActiveChannel(initialChannel);
    }

    public event EventHandler<WorkspaceChangedEventArgs>? Changed;

    public long Revision
    {
        get
        {
            lock (_gate)
            {
                return _revision;
            }
        }
    }

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
        lock (_gate)
        {
            return EnsureChannel(channelName, out _);
        }
    }

    public void SetActiveChannel(string channelName)
    {
        var channel = EnsureChannel(channelName, out var created);
        var shouldNotify = false;
        lock (_gate)
        {
            if (ActiveChannelName.Equals(channel.Name, StringComparison.OrdinalIgnoreCase))
                return;

            ActiveChannelName = channel.Name;
            channel.MarkRead();
            shouldNotify = true;
        }

        if (shouldNotify)
        {
            var kind = WorkspaceChangeKind.ActiveChannel;
            if (created)
                kind |= WorkspaceChangeKind.ChannelList;

            NotifyChanged(kind, channel.Name);
        }
    }

    public void RemoveChannel(string channelName)
    {
        channelName = NormalizeChannel(channelName);
        WorkspaceChangeKind? kind = null;
        string? changedChannel = null;
        var notify = false;
        lock (_gate)
        {
            if (_channels.Count == 1)
                return;

            var removed = _channels.Remove(channelName);
            if (!ActiveChannelName.Equals(channelName, StringComparison.OrdinalIgnoreCase))
            {
                if (!removed)
                    return;
                kind = WorkspaceChangeKind.ChannelList;
                changedChannel = channelName;
                notify = true;
            }
            else
            {
                ActiveChannelName = _channels.Keys.OrderBy(name => name).First();
                _channels[ActiveChannelName].MarkRead();
                kind = WorkspaceChangeKind.ChannelList | WorkspaceChangeKind.ActiveChannel;
                changedChannel = ActiveChannelName;
                notify = true;
            }
        }

        if (notify && kind.HasValue)
            NotifyChanged(kind.Value, changedChannel);
    }

    public void AddMessage(string channelName, Message message)
    {
        var channel = EnsureChannel(channelName, out var created);
        var isActive = channel.Name.Equals(ActiveChannelName, StringComparison.OrdinalIgnoreCase);
        channel.AddMessage(message, isActive);
        var kind = WorkspaceChangeKind.Messages;
        if (created)
            kind |= WorkspaceChangeKind.ChannelList;

        NotifyChanged(kind, channel.Name);
    }

    public void AddStatusMessage(Message message)
    {
        lock (_gate)
        {
            _statusMessages.Add(message);
            if (_statusMessages.Count > 200)
                _statusMessages.RemoveAt(0);
        }

        NotifyChanged(WorkspaceChangeKind.StatusMessages);
    }

    public void SetUsers(string channelName, IReadOnlyList<string> users)
    {
        var channel = EnsureChannel(channelName, out var created);
        channel.SetUsers(users);

        var kind = WorkspaceChangeKind.Users;
        if (created)
            kind |= WorkspaceChangeKind.ChannelList;

        NotifyChanged(kind, channel.Name);
    }

    private static string NormalizeChannel(string channel)
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw new ArgumentException("Channel cannot be empty.", nameof(channel));

        channel = channel.Trim();
        return channel.StartsWith('#') ? channel : $"#{channel}";
    }

    private ChannelBuffer EnsureChannel(string channelName, out bool created)
    {
        channelName = NormalizeChannel(channelName);
        lock (_gate)
        {
            if (_channels.TryGetValue(channelName, out var existing))
            {
                created = false;
                return existing;
            }

            var channel = new ChannelBuffer(channelName);
            _channels[channelName] = channel;
            created = true;
            return channel;
        }
    }

    private void NotifyChanged(WorkspaceChangeKind kind, string? channelName = null)
    {
        WorkspaceChangedEventArgs args;
        lock (_gate)
        {
            _revision++;
            args = new WorkspaceChangedEventArgs(kind, _revision, channelName);
        }

        Changed?.Invoke(this, args);
    }
}
