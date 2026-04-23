namespace sircceli.Core;

[Flags]
public enum WorkspaceChangeKind
{
    None = 0,
    ChannelList = 1 << 0,
    ActiveChannel = 1 << 1,
    Messages = 1 << 2,
    Users = 1 << 3,
    StatusMessages = 1 << 4,
    Topic = 1 << 5,
}
