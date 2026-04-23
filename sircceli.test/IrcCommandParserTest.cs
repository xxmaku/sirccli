using sircceli.Core.Commands;

namespace sircceli.test;

public class IrcCommandParserTest
{
    [Fact]
    public void TryParseReturnsFalseForPlainMessage()
    {
        var parsed = IrcCommandParser.TryParse("hello channel", out var command);

        Assert.False(parsed);
        Assert.Null(command);
    }

    [Fact]
    public void TryParseParsesCommandNameAndArguments()
    {
        var parsed = IrcCommandParser.TryParse("/server irc.libera.chat 6697 tls", out var command);

        Assert.True(parsed);
        Assert.NotNull(command);
        Assert.Equal("server", command.Name);
        Assert.Equal(["irc.libera.chat", "6697", "tls"], command.Arguments);
    }

    [Fact]
    public void TryParseKeepsQuotedArgumentTogether()
    {
        var parsed = IrcCommandParser.TryParse("/part #sircceli \"bye for now\"", out var command);

        Assert.True(parsed);
        Assert.NotNull(command);
        Assert.Equal(["#sircceli", "bye for now"], command.Arguments);
    }
}
