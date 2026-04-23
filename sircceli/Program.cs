using Microsoft.Extensions.Hosting;
using RazorConsole.Core;
using sircceli.Configuration;

var builder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<sircceli.UI.MainWindow>();

builder.ConfigureServices((context, services) =>
{
    services.AddSircceli(context.Configuration);
});

var host = builder.Build();

await host.RunAsync();
