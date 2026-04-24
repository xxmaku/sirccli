using Avalonia.Threading;

namespace sircceli.desktop.ViewModels;

public sealed class AvaloniaUiDispatcher : IUiDispatcher
{
    public void Invoke(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Post(action);
    }
}
