using Microsoft.Extensions.Configuration;

namespace sircceli.Configuration;

public record IrcClientConfiguration
{
    public string Server { get; init; } = string.Empty;
    public int Port { get; init; }
    public string Nick { get; init; } = string.Empty;
    public string Channel { get; init; } = string.Empty;
    public bool UseTls { get; init; }

    public static IrcClientConfiguration LoadFromAppSettings()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        return FromConfiguration(configuration);
    }

    public static IrcClientConfiguration FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        return new IrcClientConfiguration
        {
            Server = configuration["IrcClient:Server"] ?? string.Empty,
            Port = TryGetInt(configuration["IrcClient:Port"]),
            Nick = configuration["IrcClient:Nick"] ?? string.Empty,
            Channel = configuration["IrcClient:Channel"] ?? string.Empty,
            UseTls = TryGetBool(configuration["IrcClient:UseTls"])
        };
    }

    private static int TryGetInt(string? value)
        => int.TryParse(value, out var result) ? result : 0;

    private static bool TryGetBool(string? value)
        => bool.TryParse(value, out var result) && result;
}
