namespace sircceli.Models;

public class Message
{
    public required string Sender { get; init; }
    public required string Content { get; init; }
    public required DateTime Timestamp { get; init; }
}