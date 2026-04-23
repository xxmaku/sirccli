namespace sircceli.Models;

public class Message
{
    public required string Sender { get; init; }
    public required string Content { get; init; }
    public required DateTime Timestamp { get; init; }
    public string? Target { get; init; }

    public static string FormattedMessage(Message message)
    {
        var time = message.Timestamp.ToString("HH:mm:ss");
        return $"[{time}] <{message.Sender}>: {message.Content}";
    }
}
