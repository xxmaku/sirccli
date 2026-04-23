using Bunit;
using Microsoft.Extensions.DependencyInjection;
using sircceli.Core;
using sircceli.Configuration;
using sircceli.Core.Commands;
using sircceli.Models;
using sircceli.UI;

namespace sircceli.test;

public class StatusWindowTest
{
    [Fact]
    public void StatusWindowDisplaysStatusMessages()
    {
        using var ctx = new BunitContext();
        var workspace = new IrcWorkspace("#general");
        workspace.AddStatusMessage(new Message
        {
            Sender = "sircceli",
            Content = "connecting",
            Timestamp = DateTime.UtcNow
        });

        ctx.Services.AddSingleton(workspace);
        ctx.Services.AddSingleton(new IrcClientConfiguration
        {
            Server = "irc.example.org",
            Port = 6667,
            Nick = "tester",
            Channel = "#general",
            UseTls = false
        });
        ctx.Services.AddSingleton(provider => new IrcClientSession(workspace, provider.GetRequiredService<IrcClientConfiguration>()));
        ctx.Services.AddSingleton(provider => new IrcInputHandler(provider.GetRequiredService<IrcClientSession>()));

        var cut = ctx.Render<StatusWindow>();
        var text = cut.Markup;

        Assert.Contains("Getting started", text);
        Assert.Contains("/connect server [port] [nick] [#channel]", text);
        Assert.Contains("connecting", text);
    }
}
