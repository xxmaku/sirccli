namespace sircceli.Core.Network;

internal sealed class ChannelUserRoster
{
    private const char IrcPrefixMarker = ':';
    private const string NamesReplyCommand = "353";
    private const string JoinCommand = "JOIN";
    private const string PartCommand = "PART";
    private const string QuitCommand = "QUIT";
    private const string KickCommand = "KICK";
    private const string NickCommand = "NICK";

    private static readonly char[] NickPrefixCharacters = ['~', '&', '@', '%', '+'];
    private readonly List<string> _users = new();
    private readonly HashSet<string> _userLookup = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _gate = new();

    public IReadOnlyList<string> Snapshot
    {
        get
        {
            lock (_gate)
            {
                return _users.ToArray();
            }
        }
    }

    public bool TryApplyLine(string line, string channelName)
    {
        if (string.IsNullOrWhiteSpace(line) || string.IsNullOrWhiteSpace(channelName))
            return false;

        var parts = line.Split(' ', 6, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return false;

        var commandIndex = 0;
        if (parts[0].Length > 0 && parts[0][0] == IrcPrefixMarker)
            commandIndex = 1;

        if (parts.Length <= commandIndex)
            return false;

        var command = parts[commandIndex];
        var parameters = parts.Skip(commandIndex + 1).ToArray();

        if (command.Equals(NamesReplyCommand, StringComparison.OrdinalIgnoreCase))
            return TryApplyNamesReply(parameters, channelName);

        if (command.Equals(JoinCommand, StringComparison.OrdinalIgnoreCase))
            return TryApplyJoin(parts[0], parameters, channelName);

        if (command.Equals(PartCommand, StringComparison.OrdinalIgnoreCase))
            return TryApplyPart(parts[0], parameters, channelName);

        if (command.Equals(QuitCommand, StringComparison.OrdinalIgnoreCase))
            return TryApplyQuit(parts[0]);

        if (command.Equals(KickCommand, StringComparison.OrdinalIgnoreCase))
            return TryApplyKick(parameters, channelName);

        if (command.Equals(NickCommand, StringComparison.OrdinalIgnoreCase))
            return TryApplyNick(parts[0], parameters);

        return false;
    }

    private bool TryApplyNamesReply(IReadOnlyList<string> parameters, string channelName)
    {
        if (parameters.Count < 3)
            return false;

        var listedChannel = StripTrailingColon(parameters[2]);
        if (!listedChannel.Equals(channelName, StringComparison.OrdinalIgnoreCase))
            return false;

        if (parameters.Count < 4)
            return false;

        var namesSegment = StripTrailingColon(parameters[3]);
        if (string.IsNullOrWhiteSpace(namesSegment))
            return false;

        var names = namesSegment.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var changed = false;

        lock (_gate)
        {
            foreach (var rawName in names)
                changed |= AddUserLocked(rawName);
        }

        return changed;
    }

    private bool TryApplyJoin(string prefix, IReadOnlyList<string> parameters, string channelName)
    {
        if (parameters.Count < 1)
            return false;

        var joinedChannel = StripTrailingColon(parameters[0]);
        if (!joinedChannel.Equals(channelName, StringComparison.OrdinalIgnoreCase))
            return false;

        var nick = ExtractSenderName(prefix);
        if (string.IsNullOrWhiteSpace(nick))
            return false;

        lock (_gate)
        {
            return AddUserLocked(nick);
        }
    }

    private bool TryApplyPart(string prefix, IReadOnlyList<string> parameters, string channelName)
    {
        if (parameters.Count < 1)
            return false;

        var partedChannel = StripTrailingColon(parameters[0]);
        if (!partedChannel.Equals(channelName, StringComparison.OrdinalIgnoreCase))
            return false;

        var nick = ExtractSenderName(prefix);
        if (string.IsNullOrWhiteSpace(nick))
            return false;

        lock (_gate)
        {
            return RemoveUserLocked(nick);
        }
    }

    private bool TryApplyQuit(string prefix)
    {
        var nick = ExtractSenderName(prefix);
        if (string.IsNullOrWhiteSpace(nick))
            return false;

        lock (_gate)
        {
            return RemoveUserLocked(nick);
        }
    }

    private bool TryApplyKick(IReadOnlyList<string> parameters, string channelName)
    {
        if (parameters.Count < 2)
            return false;

        var kickedChannel = StripTrailingColon(parameters[0]);
        if (!kickedChannel.Equals(channelName, StringComparison.OrdinalIgnoreCase))
            return false;

        var kickedNick = StripTrailingColon(parameters[1]);
        if (string.IsNullOrWhiteSpace(kickedNick))
            return false;

        lock (_gate)
        {
            return RemoveUserLocked(kickedNick);
        }
    }

    private bool TryApplyNick(string prefix, IReadOnlyList<string> parameters)
    {
        if (parameters.Count < 1)
            return false;

        var oldNick = ExtractSenderName(prefix);
        var newNick = NormalizeNick(parameters[0]);
        if (string.IsNullOrWhiteSpace(oldNick) || string.IsNullOrWhiteSpace(newNick))
            return false;

        lock (_gate)
        {
            return RenameUserLocked(oldNick, newNick);
        }
    }

    private bool AddUserLocked(string rawUser)
    {
        var user = NormalizeNick(rawUser);
        if (string.IsNullOrWhiteSpace(user) || !_userLookup.Add(user))
            return false;

        _users.Add(user);
        return true;
    }

    private bool RemoveUserLocked(string rawUser)
    {
        var user = NormalizeNick(rawUser);
        if (string.IsNullOrWhiteSpace(user) || !_userLookup.Remove(user))
            return false;

        var index = _users.FindIndex(existing => existing.Equals(user, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
            _users.RemoveAt(index);

        return true;
    }

    private bool RenameUserLocked(string oldRawUser, string newRawUser)
    {
        var oldUser = NormalizeNick(oldRawUser);
        var newUser = NormalizeNick(newRawUser);

        if (string.IsNullOrWhiteSpace(oldUser) || string.IsNullOrWhiteSpace(newUser))
            return false;

        var index = _users.FindIndex(existing => existing.Equals(oldUser, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return false;

        if (!_userLookup.Contains(oldUser))
            return false;

        _userLookup.Remove(oldUser);

        if (oldUser.Equals(newUser, StringComparison.OrdinalIgnoreCase))
        {
            _userLookup.Add(oldUser);
            return false;
        }

        if (_userLookup.Contains(newUser))
        {
            _users.RemoveAt(index);
            return true;
        }

        _users[index] = newUser;
        _userLookup.Add(newUser);
        return true;
    }

    private static string NormalizeNick(string rawUser)
    {
        var nick = StripTrailingColon(rawUser).Trim();
        while (nick.Length > 0 && NickPrefixCharacters.Contains(nick[0]))
            nick = nick[1..];

        return nick.Trim();
    }

    private static string StripTrailingColon(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value[0] == IrcPrefixMarker ? value[1..] : value;
    }

    private static string ExtractSenderName(string senderSegment)
    {
        var sender = StripTrailingColon(senderSegment);
        var separatorIndex = sender.IndexOf('!');
        if (separatorIndex <= 0)
            return sender;

        return sender[..separatorIndex];
    }
}
