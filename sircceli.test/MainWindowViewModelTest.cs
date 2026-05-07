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
    public void NewMessagesSelectTheLatestMessage()
    {
        var workspace = new IrcWorkspace("#general");
        var session = CreateSession(workspace);
        var viewModel = CreateViewModel(workspace, session);

        var firstMessage = new Message
        {
            Sender = "alice",
            Content = "hello",
            Timestamp = DateTime.UtcNow,
            Target = "#general"
        };
        workspace.AddMessage("#general", firstMessage);

        var secondMessage = new Message
        {
            Sender = "bob",
            Content = "world",
            Timestamp = DateTime.UtcNow.AddSeconds(1),
            Target = "#general"
        };
        workspace.AddMessage("#general", secondMessage);

        Assert.NotNull(viewModel.SelectedMessage);
        Assert.Same(secondMessage, viewModel.SelectedMessage!.Message);
    }

    [Fact]
    public void PublishErrorAppearsInTheLogAndStatusCollections()
    {
        var workspace = new IrcWorkspace("#general");
        var session = CreateSession(workspace);
        var viewModel = CreateViewModel(workspace, session);

        session.PublishError("Nick tester is already in use.");

        Assert.Contains(viewModel.StatusMessages, line => line.Text.Contains("Nick tester is already in use.", StringComparison.Ordinal));
        Assert.Contains(viewModel.Messages, line => line.Text.Contains("Nick tester is already in use.", StringComparison.Ordinal));
    }

    [Fact]
    public void NewStatusMessagesSelectTheLatestStatusMessage()
    {
        var workspace = new IrcWorkspace("#general");
        var session = CreateSession(workspace);
        var viewModel = CreateViewModel(workspace, session);

        var firstMessage = new Message
        {
            Sender = "sircceli",
            Content = "Connecting to irc.example.org:6667 as tester.",
            Timestamp = DateTime.UtcNow
        };
        workspace.AddStatusMessage(firstMessage);

        var secondMessage = new Message
        {
            Sender = "sircceli",
            Content = "Connection started.",
            Timestamp = DateTime.UtcNow.AddSeconds(1)
        };
        workspace.AddStatusMessage(secondMessage);

        Assert.NotNull(viewModel.SelectedStatusMessage);
        Assert.Same(secondMessage, viewModel.SelectedStatusMessage!.Message);
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
