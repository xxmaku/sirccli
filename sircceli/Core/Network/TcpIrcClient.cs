using System.Net.Sockets;
using sircceli.Configuration;
using sircceli.Models;

namespace sircceli.Core.Network;

public sealed class TcpIrcClient : IIrcClient
{
    private string Server { get; set; }
    private int Port { get; set; }
    private string Channel { get; set; }
    private string Nick { get; set; }
    private readonly TcpClient _client = new();
    private readonly ChannelUserRoster _roster = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly object _connectGate = new();
    private Task? _connectTask;
    private StreamWriter? Writer { get; set; }

    public bool IsConnected
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            ConnectionStateChanged?.Invoke(this, field);
        }
    }

    public event EventHandler<bool>? ConnectionStateChanged;
    public event EventHandler<IReadOnlyList<string>>? ChannelUsersChanged;
    public event EventHandler<Message>? MessageReceived;
    private StreamReader? Reader { get; set; }

    public IReadOnlyList<string> CurrentUsers => _roster.Snapshot;

    public TcpIrcClient(IrcClientConfiguration cfg) : this(cfg, false)
    {
    }

    internal TcpIrcClient(bool autoConnect) : this(new IrcClientConfiguration(), autoConnect)
    {
    }

    internal TcpIrcClient(IrcClientConfiguration cfg, bool autoConnect)
    {
        ArgumentNullException.ThrowIfNull(cfg);

        Nick = cfg.Nick;
        Server = cfg.Server;
        Port = cfg.Port;
        Channel = cfg.Channel;
        if (autoConnect)
            _connectTask = Task.Run(() => ConnectAsync(_cts.Token));
    }

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        lock (_connectGate)
        {
            if (_connectTask is { IsCompleted: false })
                return Task.CompletedTask;

            var token = cancellationToken.CanBeCanceled ? cancellationToken : _cts.Token;
            _connectTask = Task.Run(() => ConnectCoreAsync(token), token);
            return Task.CompletedTask;
        }
    }

    private async Task ConnectCoreAsync(CancellationToken ct)
    {
        try
        {
            await _client.ConnectAsync(Server, Port, ct).ConfigureAwait(false);
            PublishStatus($"TCP socket connected to {Server}:{Port}.");
            Reader = new StreamReader(_client.GetStream());
            Writer = new StreamWriter(_client.GetStream()) { NewLine = "\r\n", AutoFlush = true };

            await Writer.WriteLineAsync($"NICK {Nick}").ConfigureAwait(false);
            await Writer.WriteLineAsync($"USER {Nick} 0 * :{Nick}").ConfigureAwait(false);
            PublishStatus($"Sent IRC registration for {Nick}.");

            while (!ct.IsCancellationRequested)
            {
                var line = await Reader.ReadLineAsync(ct).ConfigureAwait(false);
                if (line == null) break;

                PublishServerLine(line);

                if (!IsConnected && IrcServerLineDiagnostics.IsRegistrationWelcome(line))
                {
                    await Writer!.WriteLineAsync($"JOIN {Channel}").ConfigureAwait(false);
                    PublishStatus($"Sent JOIN {Channel}.");
                    IsConnected = true;
                }

                if (TryParsePrivmsg(line, out var decodedMessage))
                {
                    MessageReceived?.Invoke(this, decodedMessage!);
                }

                if (_roster.TryApplyLine(line, Channel))
                {
                    ChannelUsersChanged?.Invoke(this, _roster.Snapshot);
                }

                if (!line.StartsWith("PING ", StringComparison.OrdinalIgnoreCase) || Writer == null) continue;
                var payload = line.Substring(5);
                await Writer.WriteLineAsync($"PONG {payload}").ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested, no action needed.
        }
        catch (Exception ex)
        {
            IsConnected = false;
            PublishStatus($"Connection to {Server}:{Port} failed: {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            IsConnected = false;
        }
    }

    public Task DisconnectAsync()
    {
        Dispose();
        IsConnected = false;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        _client.Close();
        _client.Dispose();
        Writer?.Dispose();
        Reader?.Dispose();
    }

    public async Task SendMessage(string target, string message)
    {
        if (!IsConnected || Writer == null)
            throw new InvalidOperationException("IRC client is not connected.");

        await SendRawMessage($"PRIVMSG {target} :{message}").ConfigureAwait(false);

        var outboundMessage = new Message
        {
            Sender = Nick,
            Content = message,
            Timestamp = DateTime.UtcNow,
            Target = target
        };

        MessageReceived?.Invoke(this, outboundMessage);
    }

    public async Task SendRawMessage(string message)
    {
        if (!IsConnected || Writer == null)
            throw new InvalidOperationException("IRC client is not connected.");

        TrackJoinedChannel(message);
        PublishStatus($"> {message}");
        await Writer.WriteLineAsync(message).ConfigureAwait(false);
    }

    private void TrackJoinedChannel(string message)
    {
        if (!message.StartsWith("JOIN ", StringComparison.OrdinalIgnoreCase))
            return;

        var channel = message.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1);
        if (!string.IsNullOrWhiteSpace(channel))
            Channel = channel;
    }

    private static bool TryParsePrivmsg(string line, out Message? message)
    {
        message = null;
        if (!line.StartsWith(':'))
            return false;

        var components = line.Split(' ', 4, StringSplitOptions.RemoveEmptyEntries);
        if (components.Length < 4)
            return false;

        if (!components[1].Equals("PRIVMSG", StringComparison.OrdinalIgnoreCase))
            return false;

        var senderSegment = components[0][1..];
        var senderName = ExtractSenderName(senderSegment);
        var target = components[2];
        var contentSegment = components[3];
        if (contentSegment.StartsWith(':'))
            contentSegment = contentSegment[1..];

        message = new Message
        {
            Sender = senderName,
            Content = contentSegment,
            Timestamp = DateTime.UtcNow,
            Target = target
        };

        return true;
    }

    private static string ExtractSenderName(string senderSegment)
    {
        var separatorIndex = senderSegment.IndexOf('!');
        if (separatorIndex <= 0)
            return senderSegment;

        return senderSegment[..separatorIndex];
    }

    private void PublishServerLine(string line)
    {
        if (IrcServerLineDiagnostics.ShouldPublish(line))
            PublishStatus($"< {line}");
    }

    private void PublishStatus(string content)
        => MessageReceived?.Invoke(this, new Message
        {
            Sender = "sircceli",
            Content = content,
            Timestamp = DateTime.UtcNow,
            Target = Channel
        });
}
