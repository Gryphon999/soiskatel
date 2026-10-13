using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tests.Tracker;

public class HistoryTextTests
{
    private static readonly IReadOnlyList<HistoryEntry> Sample =
    [
        new(new DateOnly(2026, 10, 13), ApplicationStatus.Sent),
        new(new DateOnly(2026, 10, 15), ApplicationStatus.Invited),
    ];

    [Fact]
    public void Format_writes_dates_and_statuses() =>
        Assert.Equal("2026-10-13 отправлен; 2026-10-15 приглашение", HistoryText.Format(Sample));

    [Fact]
    public void TryParse_reads_formatted_history()
    {
        Assert.True(HistoryText.TryParse("2026-10-13 отправлен; 2026-10-15 приглашение", out var entries));
        Assert.Equal(Sample, entries);
    }

    [Theory]
    [InlineData("")]
    [InlineData("13.10.2026 отправлен")]
    [InlineData("2026-10-13 думаю")]
    [InlineData("2026-10-13")]
    public void TryParse_rejects_bad_history(string text) =>
        Assert.False(HistoryText.TryParse(text, out _));

    [Fact]
    public void TryParse_tolerates_extra_spaces()
    {
        Assert.True(HistoryText.TryParse("  2026-10-13   отправлен ;2026-10-15 приглашение  ", out var entries));
        Assert.Equal(Sample, entries);
    }
}
