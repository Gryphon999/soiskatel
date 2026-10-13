using System.IO.Pipelines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace Soiskatel.Mcp.Tests.TestSupport;

/// <summary>Сервер и клиент MCP в одном процессе: общаются через каналы в памяти, как через stdio.</summary>
public sealed class McpHarness : IAsyncDisposable
{
    private readonly IHost _host;

    private McpHarness(IHost host, McpClient client)
    {
        _host = host;
        Client = client;
    }

    public McpClient Client { get; }

    public static async Task<McpHarness> StartAsync(string searchFolder, TimeProvider time)
    {
        Pipe clientToServer = new(), serverToClient = new();

        var builder = Host.CreateEmptyApplicationBuilder(settings: null);
        builder.Services.AddSoiskatelCore(searchFolder, time);
        builder.Services
            .AddMcpServer()
            .WithStreamServerTransport(clientToServer.Reader.AsStream(), serverToClient.Writer.AsStream())
            .AddSoiskatelTools();

        var host = builder.Build();
        await host.StartAsync();

        var client = await McpClient.CreateAsync(
            new StreamClientTransport(clientToServer.Writer.AsStream(), serverToClient.Reader.AsStream()));

        return new McpHarness(host, client);
    }

    public async Task<CallToolResult> CallAsync(string tool, Dictionary<string, object?> arguments)
    {
        var tools = await Client.ListToolsAsync();
        return await tools.Single(t => t.Name == tool).CallAsync(arguments);
    }

    public async ValueTask DisposeAsync()
    {
        await Client.DisposeAsync();
        await _host.StopAsync();
        _host.Dispose();
    }
}
