using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tests.Tracker;

public class JobApplicationTests
{
    private static JobApplication Sample(params HistoryEntry[] history) =>
        new(new DateOnly(2026, 10, 1), "Ромашка", "C# разработчик", "hh.ru", "", history[^1].Status, history, "");

    [Fact]
    public void LastChange_is_date_of_last_history_entry()
    {
        var app = Sample(
            new(new DateOnly(2026, 10, 1), ApplicationStatus.Sent),
            new(new DateOnly(2026, 10, 5), ApplicationStatus.Invited));

        Assert.Equal(new DateOnly(2026, 10, 5), app.LastChange);
    }

    [Fact]
    public void Reached_checks_whole_history()
    {
        var app = Sample(
            new(new DateOnly(2026, 10, 1), ApplicationStatus.Sent),
            new(new DateOnly(2026, 10, 5), ApplicationStatus.Interview),
            new(new DateOnly(2026, 10, 9), ApplicationStatus.Rejected));

        Assert.True(app.Reached(ApplicationStatus.Interview));
        Assert.False(app.Reached(ApplicationStatus.Offer));
    }
}
