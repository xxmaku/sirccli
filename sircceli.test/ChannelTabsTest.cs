using Bunit;
using Microsoft.Extensions.DependencyInjection;
using sircceli.Core;
using sircceli.Models;
using sircceli.UI;

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
}
