using sircceli.Core;
using sircceli.Models;

namespace sircceli.test;

public class IrcWorkspaceTest
{
    [Fact]
    public void ConstructorUsesInitialChannel()
    {
        var workspace = new IrcWorkspace("#general");

        Assert.Equal("#general", workspace.ActiveChannelName);
        Assert.Equal("#general", workspace.ActiveChannel.Name);
    }

    [Fact]
    public void StatusMessagesAreStoredSeparately()
    {
        var workspace = new IrcWorkspace("#general");

        workspace.AddStatusMessage(new sircceli.Models.Message
        {
            Sender = "sircceli",
            Content = "connecting",
            Timestamp = DateTime.UtcNow
        });

        Assert.Single(workspace.StatusMessages);
        Assert.Empty(workspace.ActiveChannel.Messages);
    }

    [Fact]
    public void WorkspaceChangeEventsDescribeTheMutation()
    {
        var workspace = new IrcWorkspace("#general");
        WorkspaceChangedEventArgs? captured = null;
        workspace.Changed += (_, args) => captured = args;

        workspace.AddStatusMessage(new Message
        {
            Sender = "sircceli",
            Content = "connecting",
            Timestamp = DateTime.UtcNow
        });

        Assert.NotNull(captured);
        Assert.Equal(WorkspaceChangeKind.StatusMessages, captured!.Kind);
        Assert.True(captured.Revision > 0);
    }

    [Fact]
    public void AddingAMessageToANewChannelMarksTheChannelListAsChanged()
    {
        var workspace = new IrcWorkspace("#general");
        WorkspaceChangedEventArgs? captured = null;
        workspace.Changed += (_, args) => captured = args;

        workspace.AddMessage("#random", new Message
        {
            Sender = "alice",
            Content = "hello",
            Timestamp = DateTime.UtcNow,
            Target = "#random"
        });

        Assert.NotNull(captured);
        Assert.True(captured!.Kind.HasFlag(WorkspaceChangeKind.ChannelList));
        Assert.True(captured.Kind.HasFlag(WorkspaceChangeKind.Messages));
        Assert.Equal("#random", captured.ChannelName);
    }
}
