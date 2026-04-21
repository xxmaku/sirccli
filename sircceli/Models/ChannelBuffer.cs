namespace sircceli.Models;

public sealed class ChannelBuffer
{
    private const int MaxMessages = 200;
    private readonly List<Message> _messages = new();
    private readonly List<string> _users = new();
    private readonly object _gate = new();

    public ChannelBuffer(string name)
    {
        Name = NormalizeChannel(name);
    }

    public string Name { get; }
    public string Topic { get; private set; } = "Channel topic";
    public int UnreadCount { get; private set; }

    public IReadOnlyList<Message> Messages
    {
        get
        {
            lock (_gate)
            {
                return _messages.ToArray();
            }
        }
    }

    public IReadOnlyList<string> Users
    {
        get
        {
            lock (_gate)
            {
                return _users.ToArray();
            }
        }
    }

    internal void AddMessage(Message message, bool isActive)
    {
        lock (_gate)
        {
            _messages.Add(message);
            if (_messages.Count > MaxMessages)
                _messages.RemoveAt(0);

            if (!isActive)
                UnreadCount++;
        }
    }

    internal void SetUsers(IReadOnlyList<string> users)
    {
        lock (_gate)
        {
            _users.Clear();
            _users.AddRange(users);
        }
    }

    internal void SetTopic(string topic)
    {
        Topic = string.IsNullOrWhiteSpace(topic) ? "Channel topic" : topic;
    }

    internal void MarkRead()
    {
        lock (_gate)
        {
            UnreadCount = 0;
        }
    }

    private static string NormalizeChannel(string channel)
    {
        channel = channel.Trim();
        return channel.StartsWith('#') ? channel : $"#{channel}";
    }
}
