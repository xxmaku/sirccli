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
}
