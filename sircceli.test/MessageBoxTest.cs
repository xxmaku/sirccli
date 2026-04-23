using Bunit;
using Microsoft.Extensions.DependencyInjection;
using sircceli.Core;
using sircceli.Models;
using sircceli.UI;

namespace sircceli.test;

public class MessageBoxTest
{
    [Fact]
    public void MessageBoxDisplaysWorkspaceMessages()
    {
        using var ctx = new BunitContext();
        var workspace = new IrcWorkspace("#general");
        workspace.AddMessage("#general", new Message
        {
            Sender = "alice",
            Content = "hello",
            Timestamp = DateTime.UtcNow
        });

        ctx.Services.AddSingleton(workspace);

        var cut = ctx.Render<MessageBox>();

        Assert.Contains("hello", cut.Markup);
    }
}
