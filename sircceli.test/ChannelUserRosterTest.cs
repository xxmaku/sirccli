using sircceli.Core.Network;

namespace sircceli.test;

public class ChannelUserRosterTest
{
    [Fact]
    public void NamesReplySeedsRoster()
    {
        var roster = new ChannelUserRoster();

        var changed = roster.TryApplyLine(":server 353 me = #test :@alice +bob carol", "#test");
        changed |= roster.TryApplyLine(":server 366 me #test :End of /NAMES list.", "#test");

        Assert.True(changed);
        Assert.Equal(new[] { "alice", "bob", "carol" }, roster.Snapshot);
    }

    [Fact]
    public void JoinPartQuitAndKickUpdateRoster()
    {
        var roster = new ChannelUserRoster();

        roster.TryApplyLine(":server 353 me = #test :alice bob", "#test");
        roster.TryApplyLine(":server 366 me #test :End of /NAMES list.", "#test");

        Assert.True(roster.TryApplyLine(":dave!user@host JOIN #test", "#test"));
        Assert.Equal(new[] { "alice", "bob", "dave" }, roster.Snapshot);

        Assert.True(roster.TryApplyLine(":bob!user@host PART #test :leaving", "#test"));
        Assert.Equal(new[] { "alice", "dave" }, roster.Snapshot);

        Assert.True(roster.TryApplyLine(":dave!user@host QUIT :gone", "#test"));
        Assert.Equal(new[] { "alice" }, roster.Snapshot);

        Assert.True(roster.TryApplyLine(":op!user@host KICK #test alice :bye", "#test"));
        Assert.Empty(roster.Snapshot);
    }

    [Fact]
    public void NickChangesReplaceExistingUsers()
    {
        var roster = new ChannelUserRoster();

        roster.TryApplyLine(":server 353 me = #test :alice bob", "#test");
        roster.TryApplyLine(":server 366 me #test :End of /NAMES list.", "#test");

        Assert.True(roster.TryApplyLine(":bob!user@host NICK :robert", "#test"));
        Assert.Equal(new[] { "alice", "robert" }, roster.Snapshot);
    }

    [Fact]
    public void NamesReplyEndReplacesStaleNickEntries()
    {
        var roster = new ChannelUserRoster();

        roster.TryApplyLine(":server 353 me = #test :alice bob", "#test");
        roster.TryApplyLine(":server 366 me #test :End of /NAMES list.", "#test");
        roster.TryApplyLine(":bob!user@host NICK :robert", "#test");
        roster.TryApplyLine(":server 353 me = #test :alice robert carol", "#test");
        roster.TryApplyLine(":server 366 me #test :End of /NAMES list.", "#test");

        Assert.Equal(new[] { "alice", "robert", "carol" }, roster.Snapshot);
    }
}
