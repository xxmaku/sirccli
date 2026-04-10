using sircceli.Core.Network;

namespace sircceli.test;

public class ChannelUserRosterTest
{
    [Fact]
    public void NamesReplySeedsRoster()
    {
        var roster = new ChannelUserRoster();

        var changed = roster.TryApplyLine(":server 353 me = #test :@alice +bob carol", "#test");

        Assert.True(changed);
        Assert.Equal(new[] { "alice", "bob", "carol" }, roster.Snapshot);
    }

    [Fact]
    public void JoinPartQuitAndKickUpdateRoster()
    {
        var roster = new ChannelUserRoster();

        roster.TryApplyLine(":server 353 me = #test :alice bob", "#test");

        Assert.True(roster.TryApplyLine(":dave!user@host JOIN #test", "#test"));
        Assert.Equal(new[] { "alice", "bob", "dave" }, roster.Snapshot);

        Assert.True(roster.TryApplyLine(":bob!user@host PART #test :leaving", "#test"));
        Assert.Equal(new[] { "alice", "dave" }, roster.Snapshot);

        Assert.True(roster.TryApplyLine(":dave!user@host QUIT :gone", "#test"));
        Assert.Equal(new[] { "alice" }, roster.Snapshot);

        Assert.True(roster.TryApplyLine(":op!user@host KICK #test alice :bye", "#test"));
        Assert.Empty(roster.Snapshot);
    }
}
