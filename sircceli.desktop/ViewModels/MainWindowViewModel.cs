using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using sircceli.Core;
using sircceli.Core.Commands;
using sircceli.Models;

namespace sircceli.desktop.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IrcClientSession _session;
    private readonly IrcWorkspace _workspace;
    private readonly IrcInputHandler _inputHandler;
    private readonly IUiDispatcher _uiDispatcher;
    private readonly ObservableCollection<ChannelListItem> _channels = new();
    private readonly ObservableCollection<MessageLine> _messages = new();
    private readonly ObservableCollection<MessageLine> _statusMessages = new();
    private readonly ObservableCollection<string> _users = new();
    private ChannelListItem? _selectedChannel;
    private MessageLine? _selectedMessage;
    private MessageLine? _selectedStatusMessage;
    private string _draftMessage = string.Empty;
    private string _activeChannelName = string.Empty;
    private string _activeChannelTopic = string.Empty;
    private bool _isStatusVisible;
    private bool _isConnected;
    private bool _scrollMessagesToBottom;
    private bool _scrollStatusMessagesToBottom;

    public MainWindowViewModel(
        IrcClientSession session,
        IrcWorkspace workspace,
        IrcInputHandler inputHandler,
        IUiDispatcher uiDispatcher)
    {
        _session = session;
        _workspace = workspace;
        _inputHandler = inputHandler;
        _uiDispatcher = uiDispatcher;

        _session.ConnectionStateChanged += OnSessionConnectionStateChanged;
        _session.ViewStateChanged += OnSessionViewStateChanged;
        _workspace.Changed += OnWorkspaceChanged;

        RefreshFromState();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<ChannelListItem> Channels => _channels;
    public ObservableCollection<MessageLine> Messages => _messages;
    public ObservableCollection<MessageLine> StatusMessages => _statusMessages;
    public ObservableCollection<string> Users => _users;

    public string ActiveChannelName
    {
        get => _activeChannelName;
        private set => SetField(ref _activeChannelName, value);
    }

    public string ActiveChannelTopic
    {
        get => _activeChannelTopic;
        private set => SetField(ref _activeChannelTopic, value);
    }

    public string ConnectionSummary
        => _isConnected
            ? $"Connected to {_session.Configuration.Server}:{_session.Configuration.Port} as {_session.Configuration.Nick}."
            : $"Disconnected. Ready for {_session.Configuration.Server}:{_session.Configuration.Port} as {_session.Configuration.Nick}.";

    public bool IsChatVisible => !_isStatusVisible;

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (SetField(ref _isConnected, value))
            {
                OnPropertyChanged(nameof(ConnectionSummary));
            }
        }
    }

    public bool IsStatusVisible
    {
        get => _isStatusVisible;
        private set
        {
            if (SetField(ref _isStatusVisible, value))
            {
                OnPropertyChanged(nameof(IsChatVisible));
            }
        }
    }

    public ChannelListItem? SelectedChannel
    {
        get => _selectedChannel;
        set => SetSelectedChannel(value, applySessionSwitch: true);
    }

    public MessageLine? SelectedMessage
    {
        get => _selectedMessage;
        set => SetField(ref _selectedMessage, value);
    }

    public MessageLine? SelectedStatusMessage
    {
        get => _selectedStatusMessage;
        set => SetField(ref _selectedStatusMessage, value);
    }

    public string DraftMessage
    {
        get => _draftMessage;
        set => SetField(ref _draftMessage, value);
    }

    public async Task SubmitAsync()
    {
        var input = DraftMessage.Trim();
        if (string.IsNullOrWhiteSpace(input))
            return;

        try
        {
            await _inputHandler.SubmitAsync(input).ConfigureAwait(false);
            _uiDispatcher.Invoke(() =>
            {
                DraftMessage = string.Empty;
                RefreshFromState();
            });
        }
        catch (Exception ex)
        {
            _session.PublishError(ex.Message);
            _uiDispatcher.Invoke(RefreshFromState);
        }
    }

    public void Dispose()
    {
        _session.ConnectionStateChanged -= OnSessionConnectionStateChanged;
        _session.ViewStateChanged -= OnSessionViewStateChanged;
        _workspace.Changed -= OnWorkspaceChanged;
    }

    private void OnSessionConnectionStateChanged(object? sender, bool connected)
        => _uiDispatcher.Invoke(() =>
        {
            IsConnected = connected;
            OnPropertyChanged(nameof(ConnectionSummary));
        });

    private void OnSessionViewStateChanged(object? sender, EventArgs e)
        => _uiDispatcher.Invoke(RefreshFromState);

    private void OnWorkspaceChanged(object? sender, WorkspaceChangedEventArgs e)
        => _uiDispatcher.Invoke(() =>
        {
            if (e.Kind.HasFlag(WorkspaceChangeKind.Messages) &&
                e.ChannelName is not null &&
                string.Equals(e.ChannelName, _workspace.ActiveChannelName, StringComparison.OrdinalIgnoreCase))
            {
                _scrollMessagesToBottom = true;
            }

            if (e.Kind.HasFlag(WorkspaceChangeKind.StatusMessages))
            {
                _scrollStatusMessagesToBottom = true;
            }

            RefreshFromState();
        });

    private void RefreshFromState()
    {
        IsStatusVisible = _session.IsStatusVisible;
        IsConnected = _session.IsConnected;

        var activeChannel = _workspace.ActiveChannel;
        ActiveChannelName = activeChannel.Name;
        ActiveChannelTopic = activeChannel.Topic;

        SyncCollection(_channels, _workspace.GetChannels().Select(channel => new ChannelListItem(channel.Name, channel.UnreadCount)));
        SyncCollection(_messages, activeChannel.Messages.Select(message => new MessageLine(message)));
        SyncCollection(_statusMessages, _workspace.StatusMessages.Select(message => new MessageLine(message)));
        SyncCollection(_users, activeChannel.Users);

        if (_scrollMessagesToBottom)
        {
            SelectedMessage = _messages.LastOrDefault();
            _scrollMessagesToBottom = false;
        }
        else
        {
            SelectedMessage = RestoreSelectedMessage(_selectedMessage);
        }

        if (_scrollStatusMessagesToBottom)
        {
            SelectedStatusMessage = _statusMessages.LastOrDefault();
            _scrollStatusMessagesToBottom = false;
        }
        else
        {
            SelectedStatusMessage = RestoreSelectedStatusMessage(_selectedStatusMessage);
        }

        SetSelectedChannel(_channels.FirstOrDefault(channel => ChannelMatches(channel, activeChannel)), applySessionSwitch: false);

        OnPropertyChanged(nameof(ConnectionSummary));
        OnPropertyChanged(nameof(ActiveChannelName));
        OnPropertyChanged(nameof(ActiveChannelTopic));
    }

    private MessageLine? RestoreSelectedMessage(MessageLine? selectedMessage)
    {
        if (selectedMessage is null)
            return null;

        return _messages.FirstOrDefault(message => ReferenceEquals(message.Message, selectedMessage.Message));
    }

    private MessageLine? RestoreSelectedStatusMessage(MessageLine? selectedMessage)
    {
        if (selectedMessage is null)
            return null;

        return _statusMessages.FirstOrDefault(message => ReferenceEquals(message.Message, selectedMessage.Message));
    }

    private void SetSelectedChannel(ChannelListItem? value, bool applySessionSwitch)
    {
        if (ChannelMatches(_selectedChannel, value))
        {
            if (!ReferenceEquals(_selectedChannel, value))
            {
                _selectedChannel = value;
                OnPropertyChanged(nameof(SelectedChannel));
            }

            return;
        }

        _selectedChannel = value;
        OnPropertyChanged(nameof(SelectedChannel));

        if (applySessionSwitch && value is not null)
            _session.SwitchChannel(value.Name);
    }

    private static bool ChannelMatches(ChannelListItem? left, ChannelListItem? right)
        => string.Equals(left?.Name, right?.Name, StringComparison.OrdinalIgnoreCase);

    private static bool ChannelMatches(ChannelListItem? left, ChannelBuffer right)
        => string.Equals(left?.Name, right.Name, StringComparison.OrdinalIgnoreCase);

    private static void SyncCollection<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
            target.Add(item);
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
