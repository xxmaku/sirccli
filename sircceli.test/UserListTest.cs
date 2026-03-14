using Bunit;
using sircceli.UI;

namespace sircceli.test;

public class UserListTest
{
    [Fact]
    public void UserListDisplaysUsers()
    {
        using var ctx = new BunitContext();

        // Arrange & Act
        var cut = ctx.Render<UserList>();

        // Assert

        Assert.Contains("user1", cut.Markup);
        Assert.Contains("user2", cut.Markup);
        Assert.Contains("user3", cut.Markup);
    }
}