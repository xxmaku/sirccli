namespace sircceli.Configuration;

public record IrcClientConfiguration
{
    public string Server { get; init; } = "irc.freenode.org";
    public int Port { get; init; } = 6667;
    public string Nick { get; init; } = "xxmakuTest";
    public string Channel { get; init; } = "#xxmaku";
    public bool UseTls { get; init; } = false;
}