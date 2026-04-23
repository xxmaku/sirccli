namespace sircceli.Core;

public sealed class WorkspaceChangedEventArgs : EventArgs
{
    public WorkspaceChangedEventArgs(WorkspaceChangeKind kind, long revision, string? channelName = null)
    {
        Kind = kind;
        Revision = revision;
        ChannelName = channelName;
    }

    public WorkspaceChangeKind Kind { get; }
    public long Revision { get; }
    public string? ChannelName { get; }
}
