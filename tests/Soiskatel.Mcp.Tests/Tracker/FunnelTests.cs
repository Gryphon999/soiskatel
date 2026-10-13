using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tests.Tracker;

public class FunnelTests
{
    private static readonly DateOnly Today = new(2026, 10, 20);

    private static JobApplication App(string company, params (int Day, ApplicationStatus Status)[] history)
    {
        var entries = history.Select(h => new HistoryEntry(new DateOnly(2026, 10, h.Day), h.Status)).ToList();
        return new JobApplication(entries[0].Date, company, "C# разработчик", "hh.ru", "", entries[^1].Status, entries, "");
    }

    private static readonly IReadOnlyList<JobApplication> Sample =
    [
        App("А", (1, ApplicationStatus.Sent)),
        App("Б", (1, ApplicationStatus.Sent), (2, ApplicationStatus.Viewed)),
        App("В", (1, ApplicationStatus.Sent), (3, ApplicationStatus.Invited)),
        App("Г", (1, ApplicationStatus.Sent), (3, ApplicationStatus.Invited), (5, ApplicationStatus.Interview), (8, ApplicationStatus.Rejected)),
        App("Д", (1, ApplicationStatus.Sent), (2, ApplicationStatus.Invited), (4, ApplicationStatus.Interview), (9, ApplicationStatus.Offer)),
        App("Е", (1, ApplicationStatus.Sent), (2, ApplicationStatus.Rejected)),
        App("Ж", (15, ApplicationStatus.Sent)),
    ];

    [Fact]
    public void Empty_list_gives_zeros()
    {
        var stats = Funnel.Calculate([], Today);

        Assert.Equal(new[] { 0, 0, 0, 0 }, new[] { stats.Sent, stats.Responded, stats.Interviews, stats.Offers });
        Assert.Empty(stats.NoResponse);
    }

    [Fact]
    public void Counts_every_stage_by_history()
    {
        var stats = Funnel.Calculate(Sample, Today);

        Assert.Equal(7, stats.Sent);
        Assert.Equal(4, stats.Responded);
        Assert.Equal(2, stats.Interviews);
        Assert.Equal(1, stats.Offers);
    }

    [Fact]
    public void Viewed_is_not_a_response() =>
        Assert.Equal(0, Funnel.Calculate([Sample[1]], Today).Responded);

    // Решение автора. По умолчанию из спецификации: отказ тоже ответ работодателя.
    [Fact]
    public void Rejected_counts_as_response() =>
        Assert.Equal(1, Funnel.Calculate([Sample[5]], Today).Responded);

    [Fact]
    public void No_response_lists_old_sent_and_viewed_only()
    {
        var stats = Funnel.Calculate(Sample, Today);

        Assert.Equal(new[] { "А", "Б" }, stats.NoResponse.Select(a => a.Company));
    }

    [Theory]
    [InlineData(13, true)]
    [InlineData(14, false)]
    public void Seven_days_or_more_is_no_response(int sentDay, bool expected)
    {
        var stats = Funnel.Calculate([App("З", (sentDay, ApplicationStatus.Sent))], Today);

        Assert.Equal(expected, stats.NoResponse.Count == 1);
    }
}
