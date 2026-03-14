using Bunit;
using Xunit;
using sircceli.UI;

public class UserListTest
{
    [Fact]
    public void UserListDisplaysUsers()
    {
        using var ctx = new TestContext();

        // Arrange & Act
        var cut = ctx.Render<UserList>();

        // Assert
        
        Assert.Contains("user1", cut.Markup);
        Assert.Contains("user2", cut.Markup);
        Assert.Contains("user3", cut.Markup);
    }
}