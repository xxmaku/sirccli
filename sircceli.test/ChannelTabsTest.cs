using Bunit;
using Microsoft.Extensions.DependencyInjection;
using sircceli.Core;
using sircceli.Models;
using sircceli.UI;
using System.Reflection;

namespace sircceli.test;

public class ChannelTabsTest
{
    [Fact]
    public void ChannelTabsDisplaysChannelsAndUnreadCounts()
    {
        using var ctx = new BunitContext();
        var workspace = new IrcWorkspace("#general");
        workspace.EnsureChannel("#random");
        workspace.AddMessage("#random", new Message
        {
            Sender = "alice",
            Content = "hello",
            Timestamp = DateTime.UtcNow
        });

        ctx.Services.AddSingleton(workspace);

        var cut = ctx.Render<ChannelTabs>();

        Assert.Contains("#general", cut.Markup);
        Assert.Contains("#random(1)", cut.Markup);
    }

    [Fact]
    public void ChannelTabsTracksTheActiveChannelSelection()
    {
        using var ctx = new BunitContext();
        var workspace = new IrcWorkspace("#general");
        workspace.EnsureChannel("#random");

        ctx.Services.AddSingleton(workspace);

        var cut = ctx.Render<ChannelTabs>();

        Assert.Equal("#general", GetSelectedChannelName(cut));

        workspace.SetActiveChannel("#random");
        cut.Render();

        Assert.Equal("#random", GetSelectedChannelName(cut));
    }

    private static string GetSelectedChannelName(IRenderedComponent<ChannelTabs> cut)
    {
        var field = typeof(ChannelTabs).GetField("_selectedChannel", BindingFlags.Instance | BindingFlags.NonPublic);
        var channel = Assert.IsType<ChannelBuffer>(field!.GetValue(cut.Instance));

        return channel.Name;
    }
}
