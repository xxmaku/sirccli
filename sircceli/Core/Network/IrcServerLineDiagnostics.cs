namespace sircceli.Core.Network;

internal static class IrcServerLineDiagnostics
{
    private const string WelcomeReply = "001";
    private const string NicknameInUseReply = "433";
    private const string NotRegisteredReply = "451";
    private const string PasswordMismatchReply = "464";
    private const string BannedFromServerReply = "465";
    private const string NoticeCommand = "NOTICE";
    private const string ErrorCommand = "ERROR";

    public static bool ShouldPublish(string line)
        => IsRegistrationWelcome(line)
           || IsRegistrationProblem(line)
           || IsNotice(line)
           || IsError(line);

    public static bool IsNicknameInUse(string line)
        => ContainsNumericReply(line, NicknameInUseReply);

    public static string FormatNicknameInUseMessage(string nick)
        => $"Nick {nick} is already in use.";

    public static bool IsNicknameInUseMessage(string content, string nick)
        => string.Equals(content, FormatNicknameInUseMessage(nick), StringComparison.Ordinal);

    public static string BuildFallbackNick(string nick, int attempt)
    {
        if (string.IsNullOrWhiteSpace(nick))
            throw new ArgumentException("Nick cannot be empty.", nameof(nick));

        if (attempt is < 1 or > 99)
            throw new ArgumentOutOfRangeException(nameof(attempt), "Attempt must be between 1 and 99.");

        return $"{nick.Trim()}{attempt}";
    }

    public static bool IsConnectionFailureMessage(string content)
        => content.Contains("failed:", StringComparison.OrdinalIgnoreCase);

    public static bool IsRegistrationWelcome(string line)
        => ContainsNumericReply(line, WelcomeReply);

    private static bool IsRegistrationProblem(string line)
        => ContainsNumericReply(line, NicknameInUseReply)
           || ContainsNumericReply(line, NotRegisteredReply)
           || ContainsNumericReply(line, PasswordMismatchReply)
           || ContainsNumericReply(line, BannedFromServerReply);

    private static bool IsNotice(string line)
        => ContainsCommand(line, NoticeCommand);

    private static bool IsError(string line)
        => line.StartsWith($"{ErrorCommand} ", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsNumericReply(string line, string numeric)
        => ContainsCommand(line, numeric);

    private static bool ContainsCommand(string line, string command)
        => line.Contains($" {command} ", StringComparison.OrdinalIgnoreCase);
}
