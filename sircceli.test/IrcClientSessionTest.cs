using sircceli.Configuration;
using sircceli.Core;

namespace sircceli.test;

public class IrcClientSessionTest
{
    [Fact]
    public async Task JoinAsyncHidesInitialStatusWindow()
    {
        var workspace = new IrcWorkspace("#general");
        var session = new IrcClientSession(workspace, new IrcClientConfiguration
        {
            Server = "irc.example.org",
            Port = 6667,
            Nick = "tester",
            Channel = "#general",
            UseTls = false
        });

        Assert.True(session.IsStatusVisible);

        await session.JoinAsync("#general");

        Assert.False(session.IsStatusVisible);
        Assert.Equal("#general", workspace.ActiveChannelName);
    }
}
