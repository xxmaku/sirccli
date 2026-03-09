namespace sircceli.Core.Events;

public class EventsMain
{
    public event Action<string> OnConnected;
    public event Action<string> OnDisconnected;
}