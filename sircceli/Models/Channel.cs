namespace sircceli.Models;

public class Channel
{
    public required string Name { get; init; }
    public required string Topic { get; init; }
    public required List<string> Users { get; init; }
}