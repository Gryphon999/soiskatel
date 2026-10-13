using Microsoft.Extensions.Time.Testing;
using ModelContextProtocol.Protocol;
using Soiskatel.Mcp.Tests.TestSupport;

namespace Soiskatel.Mcp.Tests;

public class McpSmokeTests
{
    private static FakeTimeProvider Clock() => new(new DateTimeOffset(2026, 10, 13, 9, 0, 0, TimeSpan.Zero));

    private static string Text(CallToolResult result) =>
        string.Join("\n", result.Content.OfType<TextContentBlock>().Select(c => c.Text));

    [Fact]
    public async Task Server_exposes_five_tools()
    {
        using var temp = new TempFolder();
        await using var mcp = await McpHarness.StartAsync(temp.Root, Clock());

        var names = (await mcp.Client.ListToolsAsync()).Select(t => t.Name).Order().ToList();

        Assert.Equal(
            new[] { "text_check", "tracker_add", "tracker_find_company", "tracker_stats", "tracker_update" },
            names);
    }

    [Fact]
    public async Task Text_check_returns_cleaned_text()
    {
        using var temp = new TempFolder();
        await using var mcp = await McpHarness.StartAsync(temp.Root, Clock());

        var result = await mcp.CallAsync("text_check", new() { ["text"] = "Москва — Ереван, 9–18" });

        Assert.Contains("Москва, Ереван, 9-18", Text(result));
    }

    [Fact]
    public async Task Tracker_flow_works_through_mcp()
    {
        using var temp = new TempFolder();
        await using var mcp = await McpHarness.StartAsync(temp.Root, Clock());

        await mcp.CallAsync("tracker_add", new() { ["company"] = "Ромашка Софт", ["position"] = "Junior C# разработчик", ["platform"] = "hh.ru" });
        await mcp.CallAsync("tracker_update", new() { ["status"] = "приглашение", ["company"] = "Ромашка Софт" });
        var stats = await mcp.CallAsync("tracker_stats", new());

        Assert.Contains("Ответили: 1 (100%)", Text(stats));
        Assert.True(File.Exists(temp.File("applications.md")));
    }

    [Fact]
    public async Task Forbidden_transition_is_a_tool_error_with_reason()
    {
        using var temp = new TempFolder();
        await using var mcp = await McpHarness.StartAsync(temp.Root, Clock());
        await mcp.CallAsync("tracker_add", new() { ["company"] = "Ромашка Софт", ["position"] = "Junior C# разработчик" });
        await mcp.CallAsync("tracker_update", new() { ["status"] = "собеседование", ["company"] = "Ромашка Софт" });

        var result = await mcp.CallAsync("tracker_update", new() { ["status"] = "отправлен", ["company"] = "Ромашка Софт" });

        Assert.True(result.IsError is true);
        Assert.Contains("Нельзя сменить статус", Text(result));
    }

    [Fact]
    public async Task Locked_file_is_a_clear_tool_error()
    {
        using var temp = new TempFolder();
        await using var mcp = await McpHarness.StartAsync(temp.Root, Clock());
        await mcp.CallAsync("tracker_add", new() { ["company"] = "Ромашка Софт", ["position"] = "Junior C# разработчик" });

        using (new FileStream(temp.File("applications.md"), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var result = await mcp.CallAsync("tracker_stats", new());

            Assert.True(result.IsError is true);
            Assert.Contains("файл занят", Text(result));
        }
    }
}
