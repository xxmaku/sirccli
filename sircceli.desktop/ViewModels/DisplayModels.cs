using sircceli.Models;

namespace sircceli.desktop.ViewModels;

public sealed class ChannelListItem
{
    public ChannelListItem(string name, int unreadCount)
    {
        Name = name;
        UnreadCount = unreadCount;
    }

    public string Name { get; }
    public int UnreadCount { get; }

    public string DisplayText => UnreadCount > 0 ? $"{Name} ({UnreadCount})" : Name;
}

public sealed class MessageLine
{
    public MessageLine(Message message)
    {
        Message = message;
        Text = Message.FormattedMessage(message);
    }

    public Message Message { get; }
    public string Text { get; }
}
