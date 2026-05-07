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

    [Fact]
    public async Task NicknameInUseMessageShowsTheStatusWindow()
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

        await session.JoinAsync("#general");

        session.HandleClientMessage(new Models.Message
        {
            Sender = "sircceli",
            Content = "Nick tester is already in use.",
            Timestamp = DateTime.UtcNow
        });

        Assert.True(session.IsStatusVisible);
    }

    [Fact]
    public async Task ConnectionFailureMessageShowsTheStatusWindow()
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

        await session.JoinAsync("#general");

        session.HandleClientMessage(new Models.Message
        {
            Sender = "sircceli",
            Content = "Connection to irc.example.org:6667 failed: SocketException: refused",
            Timestamp = DateTime.UtcNow
        });

        Assert.True(session.IsStatusVisible);
    }
}
