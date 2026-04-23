using Bunit;
using Microsoft.Extensions.DependencyInjection;
using sircceli.Core;
using sircceli.UI;

namespace sircceli.test;

public class UserListTest
{
    [Fact]
    public void UserListDisplaysProvidedUsers()
    {
        using var ctx = new BunitContext();
        ctx.Services.AddSingleton(new IrcWorkspace("#general"));

        var cut = ctx.Render<UserList>(parameters => parameters
            .Add(p => p.Users, new[] { "user1", "user2", "user3" }));

        Assert.Contains("user1", cut.Markup);
        Assert.Contains("user2", cut.Markup);
        Assert.Contains("user3", cut.Markup);
    }
}
