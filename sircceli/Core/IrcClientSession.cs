using sircceli.Configuration;
using sircceli.Core.Network;
using sircceli.Models;

namespace sircceli.Core;

public sealed class IrcClientSession : IIrcClient
{
    private readonly IrcClientFactory _factory;
    private readonly IrcWorkspace _workspace;
    private readonly object _gate = new();
    private IIrcClient? _client;

    public IrcClientSession(IrcClientFactory factory, IrcWorkspace workspace)
    {
        _factory = factory;
        _workspace = workspace;
        _workspace.SetActiveChannel(Configuration.Channel);
    }

    public IrcClientConfiguration Configuration { get; private set; } = new();

    public bool IsConnected => _client?.IsConnected == true;

    public IReadOnlyList<string> CurrentUsers => _client?.CurrentUsers ?? Array.Empty<string>();

    public event EventHandler<bool>? ConnectionStateChanged;
    public event EventHandler<IReadOnlyList<string>>? ChannelUsersChanged;
    public event EventHandler<Message>? MessageReceived;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        IIrcClient client;
        lock (_gate)
        {
            if (_client?.IsConnected == true)
            {
                PublishStatus("Already connected.");
                return;
            }

            ReplaceClientLocked(_factory.Create(Configuration));
            client = _client!;
        }

        PublishStatus($"Connecting to {Configuration.Server}:{Configuration.Port} as {Configuration.Nick}.");
        await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
        PublishStatus("Connection started.");
    }

    public async Task ConnectAsync(IrcClientConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Configuration = configuration;
        if (_client != null)
            await DisconnectAsync().ConfigureAwait(false);

        await ConnectAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task DisconnectAsync()
    {
        IIrcClient? oldClient;
        lock (_gate)
        {
            oldClient = _client;
            if (oldClient == null)
            {
                PublishStatus("Already disconnected.");
                return Task.CompletedTask;
            }

            Unsubscribe(oldClient);
            _client = null;
        }

        oldClient.Dispose();
        ChannelUsersChanged?.Invoke(this, Array.Empty<string>());
        ConnectionStateChanged?.Invoke(this, false);
        PublishStatus("Disconnected.");
        return Task.CompletedTask;
    }

    public async Task JoinAsync(string channel)
    {
        channel = NormalizeChannel(channel);
        Configuration = Configuration with { Channel = channel };
        _workspace.SetActiveChannel(channel);

        if (!IsConnected)
        {
            PublishStatus($"Channel set to {channel}.");
            return;
        }

        await SendRawMessage($"JOIN {channel}").ConfigureAwait(false);
        PublishStatus($"Joining {channel}.");
    }

    public async Task PartAsync(string? channel = null, string? reason = null)
    {
        var targetChannel = NormalizeChannel(string.IsNullOrWhiteSpace(channel) ? Configuration.Channel : channel);
        var command = string.IsNullOrWhiteSpace(reason)
            ? $"PART {targetChannel}"
            : $"PART {targetChannel} :{reason}";

        if (IsConnected)
            await SendRawMessage(command).ConfigureAwait(false);

        _workspace.RemoveChannel(targetChannel);
        PublishStatus($"Parting {targetChannel}.");
    }

    public void SwitchChannel(string channel)
    {
        channel = NormalizeChannel(channel);
        _workspace.SetActiveChannel(channel);
        PublishStatus($"Switched to {channel}.");
    }

    public async Task ChangeNickAsync(string nick)
    {
        if (string.IsNullOrWhiteSpace(nick))
            throw new ArgumentException("Nick cannot be empty.", nameof(nick));

        Configuration = Configuration with { Nick = nick };

        if (!IsConnected)
        {
            PublishStatus($"Nick set to {nick}.");
            return;
        }

        await SendRawMessage($"NICK {nick}").ConfigureAwait(false);
        PublishStatus($"Nick changed to {nick}.");
    }

    public void SetServer(string server, int? port = null, bool? useTls = null)
    {
        if (string.IsNullOrWhiteSpace(server))
            throw new ArgumentException("Server cannot be empty.", nameof(server));

        Configuration = Configuration with
        {
            Server = server,
            Port = port ?? Configuration.Port,
            UseTls = useTls ?? Configuration.UseTls
        };

        PublishStatus($"Server set to {Configuration.Server}:{Configuration.Port}.");
    }

    public async Task SendMessage(string message)
        => await SendMessage(_workspace.ActiveChannel.Name, message).ConfigureAwait(false);

    public async Task SendMessage(string target, string message)
    {
        if (_client == null)
            throw new InvalidOperationException("IRC client is not connected.");

        await _client.SendMessage(NormalizeChannel(target), message).ConfigureAwait(false);
    }

    public async Task SendRawMessage(string message)
    {
        if (_client == null)
            throw new InvalidOperationException("IRC client is not connected.");

        await _client.SendRawMessage(message).ConfigureAwait(false);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    private void ReplaceClientLocked(IIrcClient client)
    {
        if (_client != null)
        {
            Unsubscribe(_client);
            _client.Dispose();
        }

        _client = client;
        _client.ConnectionStateChanged += OnClientConnectionStateChanged;
        _client.ChannelUsersChanged += OnClientChannelUsersChanged;
        _client.MessageReceived += OnClientMessageReceived;
    }

    private void OnClientConnectionStateChanged(object? sender, bool connected)
        => ConnectionStateChanged?.Invoke(this, connected);

    private void OnClientChannelUsersChanged(object? sender, IReadOnlyList<string> users)
    {
        _workspace.SetUsers(Configuration.Channel, users);
        ChannelUsersChanged?.Invoke(this, users);
    }

    private void OnClientMessageReceived(object? sender, Message message)
    {
        var target = string.IsNullOrWhiteSpace(message.Target) ? _workspace.ActiveChannel.Name : message.Target;
        _workspace.AddMessage(target, message);
        MessageReceived?.Invoke(this, message);
    }

    private void Unsubscribe(IIrcClient client)
    {
        client.ConnectionStateChanged -= OnClientConnectionStateChanged;
        client.ChannelUsersChanged -= OnClientChannelUsersChanged;
        client.MessageReceived -= OnClientMessageReceived;
    }

    private void PublishStatus(string content)
    {
        var message = new Message
        {
            Sender = "sircceli",
            Content = content,
            Timestamp = DateTime.UtcNow,
            Target = _workspace.ActiveChannel.Name
        };

        _workspace.AddMessage(_workspace.ActiveChannel.Name, message);
        MessageReceived?.Invoke(this, message);
    }

    public void PublishError(string content)
        => PublishStatus(content);

    private static string NormalizeChannel(string channel)
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw new ArgumentException("Channel cannot be empty.", nameof(channel));

        channel = channel.Trim();
        return channel.StartsWith('#') ? channel : $"#{channel}";
    }
}
