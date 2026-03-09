using Microsoft.Extensions.Hosting;
using RazorConsole.Core;

var builder = Host.CreateDefaultBuilder(args)
    .UseRazorConsole<sircceli.UI.Counter>();

var host = builder.Build();

await host.RunAsync();