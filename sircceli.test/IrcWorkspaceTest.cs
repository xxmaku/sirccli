using sircceli.Core;

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
}
