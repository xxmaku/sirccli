using sircceli.Core;
using sircceli.Core.Commands;
using sircceli.desktop.ViewModels;
using sircceli.Configuration;
using sircceli.Models;

namespace sircceli.test;

public class MainWindowViewModelTest
{
    [Fact]
    public void ViewModelTracksWorkspaceChanges()
    {
        var workspace = new IrcWorkspace("#general");
        var session = CreateSession(workspace);
        var viewModel = CreateViewModel(workspace, session);

        workspace.EnsureChannel("#random");
        workspace.AddMessage("#random", new Message
        {
            Sender = "alice",
            Content = "hello",
            Timestamp = DateTime.UtcNow,
            Target = "#random"
        });

        Assert.Contains(viewModel.Channels, channel => channel.Name == "#random" && channel.UnreadCount == 1);
        Assert.Equal("#general", viewModel.SelectedChannel?.Name);
    }

    [Fact]
    public async Task JoinCommandUpdatesTheActiveChannelAndHidesStatusView()
    {
        var workspace = new IrcWorkspace("#general");
        var session = CreateSession(workspace);
        var viewModel = CreateViewModel(workspace, session);

        viewModel.DraftMessage = "/join #random";

        await viewModel.SubmitAsync();

        Assert.Equal(string.Empty, viewModel.DraftMessage);
        Assert.False(viewModel.IsStatusVisible);
        Assert.Equal("#random", viewModel.ActiveChannelName);
        Assert.Equal("#random", viewModel.SelectedChannel?.Name);
    }

    [Fact]
    public void SelectingAChannelSwitchesTheWorkspace()
    {
        var workspace = new IrcWorkspace("#general");
        workspace.EnsureChannel("#random");

        var session = CreateSession(workspace);
        var viewModel = CreateViewModel(workspace, session);

        viewModel.SelectedChannel = viewModel.Channels.Single(channel => channel.Name == "#random");

        Assert.Equal("#random", workspace.ActiveChannelName);
        Assert.Equal("#random", viewModel.ActiveChannelName);
    }

    private static MainWindowViewModel CreateViewModel(IrcWorkspace workspace, IrcClientSession session)
        => new(session, workspace, new IrcInputHandler(session), new ImmediateUiDispatcher());

    private static IrcClientSession CreateSession(IrcWorkspace workspace)
        => new(workspace, new IrcClientConfiguration
        {
            Server = "irc.example.org",
            Port = 6667,
            Nick = "tester",
            Channel = "#general",
            UseTls = false
        });

    private sealed class ImmediateUiDispatcher : IUiDispatcher
    {
        public void Invoke(Action action)
            => action();
    }
}
