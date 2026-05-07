using sircceli.Core.Network;

namespace sircceli.test;

public class IrcServerLineDiagnosticsTest
{
    [Fact]
    public void NicknameInUseReplyIsDetected()
    {
        var line = ":irc.example.org 433 tester :Nickname is already in use.";

        Assert.True(IrcServerLineDiagnostics.IsNicknameInUse(line));
    }

    [Fact]
    public void NicknameInUseMessageIsFormattedForTheCurrentNick()
    {
        Assert.Equal("Nick tester is already in use.", IrcServerLineDiagnostics.FormatNicknameInUseMessage("tester"));
    }

    [Fact]
    public void FallbackNickAppendsTheAttemptNumber()
    {
        Assert.Equal("tester1", IrcServerLineDiagnostics.BuildFallbackNick("tester", 1));
        Assert.Equal("tester99", IrcServerLineDiagnostics.BuildFallbackNick("tester", 99));
    }
}
