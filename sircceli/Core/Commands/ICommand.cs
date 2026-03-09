namespace sircceli.Core.Commands;

/// <summary>
/// Interface for commands that can be executed in the application.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// The command name.
    /// </summary>
    public string Name { get; init; }
    
    /// <summary>
    /// The command arguments.
    /// </summary>
    public List<string>? Arguments { get; init; }
}