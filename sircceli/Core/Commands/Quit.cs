namespace sircceli.Core.Commands;

public class Quit : ICommand
{
    public string Name { get; init; } = nameof(CommandType.Quit).ToUpper();
    public List<string>? Arguments { get; init; }
}