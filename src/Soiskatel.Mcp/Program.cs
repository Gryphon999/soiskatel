using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Soiskatel.Mcp;
using Soiskatel.Mcp.Storage;

// Аргументы командной строки не передаём: dnx может добавить служебные флаги.
var builder = Host.CreateEmptyApplicationBuilder(settings: null);

// Лог только в stderr: stdout занят протоколом MCP.
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

var searchFolder = SearchFolder.ResolveRoot(
    Environment.GetEnvironmentVariable("SOISKATEL_HOME"),
    Environment.CurrentDirectory);

builder.Services.AddSoiskatelCore(searchFolder, TimeProvider.System);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .AddSoiskatelTools();

await builder.Build().RunAsync();
