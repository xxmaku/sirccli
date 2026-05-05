using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using sircceli.desktop.ViewModels;

namespace sircceli.desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnSendClick(object? sender, RoutedEventArgs e)
    {
        await SubmitAsync();
    }

    private async void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || sender is not TextBox)
            return;

        e.Handled = true;
        await SubmitAsync();
    }

    private async Task SubmitAsync()
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        await viewModel.SubmitAsync();
    }
}
