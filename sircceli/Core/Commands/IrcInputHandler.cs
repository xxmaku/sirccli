using sircceli.Configuration;

namespace sircceli.Core.Commands;

public sealed class IrcInputHandler
{
    private readonly IrcClientSession _session;
    private readonly IrcCommandParser _parser;

    public IrcInputHandler(IrcClientSession session, IrcCommandParser parser)
    {
        _session = session;
        _parser = parser;
    }

    public async Task SubmitAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return;

        if (!_parser.TryParse(input, out var command) || command == null)
        {
            await _session.SendMessage(input).ConfigureAwait(false);
            return;
        }

        await ExecuteCommandAsync(command).ConfigureAwait(false);
    }

    private async Task ExecuteCommandAsync(ParsedIrcCommand command)
    {
        switch (command.Name)
        {
            case "connect":
                await _session.ConnectAsync(BuildConnectConfiguration(command.Arguments)).ConfigureAwait(false);
                break;
            case "disconnect":
            case "quit":
                await _session.DisconnectAsync().ConfigureAwait(false);
                break;
            case "join":
                RequireArgument(command, "channel");
                await _session.JoinAsync(command.Arguments[0]).ConfigureAwait(false);
                break;
            case "part":
                await _session.PartAsync(command.Arguments.FirstOrDefault(), JoinRemaining(command.Arguments, 1)).ConfigureAwait(false);
                break;
            case "channel":
            case "switch":
                RequireArgument(command, "channel");
                _session.SwitchChannel(command.Arguments[0]);
                break;
            case "nick":
                RequireArgument(command, "nick");
                await _session.ChangeNickAsync(command.Arguments[0]).ConfigureAwait(false);
                break;
            case "server":
                RequireArgument(command, "server");
                ApplyServerCommand(command.Arguments);
                break;
            default:
                throw new InvalidOperationException($"Unknown command: /{command.Name}");
        }
    }

    private IrcClientConfiguration BuildConnectConfiguration(IReadOnlyList<string> arguments)
    {
        var configuration = _session.Configuration;

        if (arguments.Count > 0)
            configuration = configuration with { Server = arguments[0] };

        if (arguments.Count > 1 && int.TryParse(arguments[1], out var port))
            configuration = configuration with { Port = port };

        foreach (var argument in arguments.Skip(2))
        {
            if (TryParseTls(argument, out var tls))
            {
                configuration = configuration with { UseTls = tls };
                continue;
            }

            configuration = argument.StartsWith('#')
                ? configuration with { Channel = NormalizeChannel(argument) }
                : configuration with { Nick = argument };
        }

        return configuration;
    }

    private void ApplyServerCommand(IReadOnlyList<string> arguments)
    {
        int? port = null;
        bool? useTls = null;

        foreach (var argument in arguments.Skip(1))
        {
            if (TryParseTls(argument, out var tls))
            {
                useTls = tls;
                continue;
            }

            port = ParseOptionalPort(argument);
        }

        _session.SetServer(arguments[0], port, useTls);
    }

    private static int? ParseOptionalPort(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (!int.TryParse(value, out var port))
            throw new InvalidOperationException($"Invalid port: {value}");

        return port;
    }

    private static bool? ParseOptionalTls(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.ToLowerInvariant() switch
        {
            "tls" or "ssl" or "true" or "on" => true,
            "notls" or "plain" or "false" or "off" => false,
            _ => throw new InvalidOperationException($"Invalid TLS option: {value}")
        };
    }

    private static bool TryParseTls(string value, out bool useTls)
    {
        switch (value.ToLowerInvariant())
        {
            case "tls":
            case "ssl":
            case "true":
            case "on":
                useTls = true;
                return true;
            case "notls":
            case "plain":
            case "false":
            case "off":
                useTls = false;
                return true;
            default:
                useTls = false;
                return false;
        }
    }

    private static void RequireArgument(ParsedIrcCommand command, string name)
    {
        if (command.Arguments.Count == 0)
            throw new InvalidOperationException($"/{command.Name} requires a {name}.");
    }

    private static string? JoinRemaining(IReadOnlyList<string> arguments, int startIndex)
    {
        if (arguments.Count <= startIndex)
            return null;

        return string.Join(' ', arguments.Skip(startIndex));
    }

    private static string NormalizeChannel(string channel)
        => channel.StartsWith('#') ? channel : $"#{channel}";
}
