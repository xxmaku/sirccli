using System.Net.Sockets;
using sircceli.Models;

namespace sircceli.Core.Network;

internal sealed class IrcClient : IDisposable
{
    private const string Server = "irc.freenode.org";
    private const int Port = 6667;
    private const string Channel = "#xxmaku";
    private const string Nick = "xxmakuTest";
    private readonly TcpClient _client;
    private readonly CancellationTokenSource _cts = new();
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
    public event EventHandler<Message>? MessageReceived;
    private StreamReader? Reader { get; set; }

    public IrcClient()
    {
        _client = new TcpClient();
        _ = Task.Run(() => ConnectAsync(_cts.Token));
    }

    private async Task ConnectAsync(CancellationToken ct)
    {
        try
        {
            await _client.ConnectAsync(Server, Port, ct).ConfigureAwait(false);
            Reader = new StreamReader(_client.GetStream());
            Writer = new StreamWriter(_client.GetStream()) { NewLine = "\r\n", AutoFlush = true };

            await Writer.WriteLineAsync($"NICK {Nick}").ConfigureAwait(false);
            await Writer.WriteLineAsync($"USER {Nick} 0 * :{Nick}").ConfigureAwait(false);

            while (!ct.IsCancellationRequested)
            {
                var line = await Reader.ReadLineAsync(ct).ConfigureAwait(false);
                if (line == null) break;

                if (!IsConnected && line.Contains(" 001 "))
                {
                    await Writer!.WriteLineAsync($"JOIN {Channel}").ConfigureAwait(false);
                    IsConnected = true;
                }

                if (TryParsePrivmsg(line, out var decodedMessage))
                {
                    MessageReceived?.Invoke(this, decodedMessage!);
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
        catch (Exception)
        {
            IsConnected = false;
        }
    }

    public void Dispose()
    {
        _client.Dispose();
        _cts.Dispose();
        Writer?.Dispose();
        Reader?.Dispose();
        _cts.Cancel();
        _client.Close();
    }

    public async Task SendMessage(string message)
    {
        if (!IsConnected || Writer == null)
            throw new InvalidOperationException("IRC client is not connected.");

        await Writer.WriteLineAsync($"PRIVMSG {Channel} :{message}").ConfigureAwait(false);

        var outboundMessage = new Message
        {
            Sender = Nick,
            Content = message,
            Timestamp = DateTime.UtcNow
        };

        MessageReceived?.Invoke(this, outboundMessage);
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
        var contentSegment = components[3];
        if (contentSegment.StartsWith(':'))
            contentSegment = contentSegment[1..];

        message = new Message
        {
            Sender = senderName,
            Content = contentSegment,
            Timestamp = DateTime.UtcNow
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
}
