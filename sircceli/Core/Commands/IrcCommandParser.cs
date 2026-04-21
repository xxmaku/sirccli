using System.Text;

namespace sircceli.Core.Commands;

public sealed class IrcCommandParser
{
    public static bool TryParse(string input, out ParsedIrcCommand? command)
    {
        command = null;
        if (string.IsNullOrWhiteSpace(input) || !input.StartsWith('/'))
            return false;

        var tokens = Tokenize(input[1..]);
        if (tokens.Count == 0)
            return false;

        command = new ParsedIrcCommand(tokens[0].ToLowerInvariant(), tokens.Skip(1).ToArray());
        return true;
    }

    private static List<string> Tokenize(string input)
    {
        var tokens = new List<string>();
        var token = new StringBuilder();
        var inQuotes = false;

        foreach (var character in input)
        {
            if (character == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(character) && !inQuotes)
            {
                AddToken();
                continue;
            }

            token.Append(character);
        }

        AddToken();
        return tokens;

        void AddToken()
        {
            if (token.Length == 0)
                return;

            tokens.Add(token.ToString());
            token.Clear();
        }
    }
}
