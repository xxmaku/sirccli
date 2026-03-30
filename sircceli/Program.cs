using Microsoft.Extensions.Hosting;
using RazorConsole.Core;
using sircceli.Configuration;

var builder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<sircceli.UI.MainWindow>();

builder.ConfigureServices((_, services) =>
{
    services.AddSircceli();
});

var host = builder.Build();

await host.RunAsync();