using Bunit;
using Microsoft.Extensions.DependencyInjection;
using sircceli.Configuration;
using sircceli.Core;
using sircceli.Core.Commands;
using sircceli.Models;
using sircceli.UI;

namespace sircceli.test;

public class MainWindowTest
{
    [Fact]
    public async Task MainWindowDisplaysCurrentWorkspaceState()
    {
        using var ctx = new BunitContext();
        var workspace = new IrcWorkspace("#general");
        var configuration = new IrcClientConfiguration
        {
            Server = "irc.example.org",
            Port = 6667,
            Nick = "tester",
            Channel = "#general",
            UseTls = false
        };
        var session = new IrcClientSession(workspace, configuration);

        ctx.Services.AddSingleton(workspace);
        ctx.Services.AddSingleton(configuration);
        ctx.Services.AddSingleton(session);
        ctx.Services.AddSingleton<IrcInputHandler>();

        await session.JoinAsync("#general");
        workspace.EnsureChannel("#random");
        workspace.AddMessage("#random", new Message
        {
            Sender = "alice",
            Content = "hello",
            Timestamp = DateTime.UtcNow,
            Target = "#random"
        });

        var cut = ctx.Render<MainWindow>();

        Assert.Contains("#random(1)", cut.Markup);
    }

    [Fact]
    public async Task MainWindowKeepsTextboxInstanceStableAcrossChannelSwitches()
    {
        using var ctx = new BunitContext();
        var workspace = new IrcWorkspace("#general");
        var configuration = new IrcClientConfiguration
        {
            Server = "irc.example.org",
            Port = 6667,
            Nick = "tester",
            Channel = "#general",
            UseTls = false
        };
        var session = new IrcClientSession(workspace, configuration);

        ctx.Services.AddSingleton(workspace);
        ctx.Services.AddSingleton(configuration);
        ctx.Services.AddSingleton(session);
        ctx.Services.AddSingleton<IrcInputHandler>();

        workspace.EnsureChannel("#random");
        var cut = ctx.Render<MainWindow>();
        var initialTextbox = cut.FindComponent<TextBox>().Instance;

        workspace.SetActiveChannel("#random");
        cut.Render();

        var updatedTextbox = cut.FindComponent<TextBox>().Instance;

        Assert.Same(initialTextbox, updatedTextbox);
    }

    [Fact]
    public async Task MainWindowRefreshesMessagePaneWhenWorkspaceChanges()
    {
        using var ctx = new BunitContext();
        var workspace = new IrcWorkspace("#general");
        var configuration = new IrcClientConfiguration
        {
            Server = "irc.example.org",
            Port = 6667,
            Nick = "tester",
            Channel = "#general",
            UseTls = false
        };
        var session = new IrcClientSession(workspace, configuration);

        ctx.Services.AddSingleton(workspace);
        ctx.Services.AddSingleton(configuration);
        ctx.Services.AddSingleton(session);
        ctx.Services.AddSingleton<IrcInputHandler>();

        await session.JoinAsync("#general");
        var cut = ctx.Render<MainWindow>();

        workspace.AddMessage("#general", new Message
        {
            Sender = "alice",
            Content = "hello",
            Timestamp = DateTime.UtcNow,
            Target = "#general"
        });

        cut.Render();

        Assert.Contains("hello", cut.Markup);
    }
}
