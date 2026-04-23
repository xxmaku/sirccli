namespace sircceli.Core.Commands;

public sealed record ParsedIrcCommand(string Name, IReadOnlyList<string> Arguments);
