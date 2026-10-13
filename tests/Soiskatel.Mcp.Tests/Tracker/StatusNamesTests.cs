using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tests.Tracker;

public class StatusNamesTests
{
    [Theory]
    [InlineData(ApplicationStatus.Sent, "отправлен")]
    [InlineData(ApplicationStatus.Viewed, "просмотрен")]
    [InlineData(ApplicationStatus.Invited, "приглашение")]
    [InlineData(ApplicationStatus.Interview, "собеседование")]
    [InlineData(ApplicationStatus.TestTask, "тестовое")]
    [InlineData(ApplicationStatus.Offer, "оффер")]
    [InlineData(ApplicationStatus.Rejected, "отказ")]
    [InlineData(ApplicationStatus.Withdrawn, "отозван")]
    public void Russian_names_round_trip(ApplicationStatus status, string name)
    {
        Assert.Equal(name, StatusNames.ToRussian(status));
        Assert.True(StatusNames.TryParse(name, out var parsed));
        Assert.Equal(status, parsed);
    }

    [Fact]
    public void TryParse_ignores_case_and_spaces()
    {
        Assert.True(StatusNames.TryParse("  Отказ ", out var status));
        Assert.Equal(ApplicationStatus.Rejected, status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("думаю")]
    public void TryParse_rejects_unknown(string? text) =>
        Assert.False(StatusNames.TryParse(text, out _));

    [Fact]
    public void AllRussian_lists_statuses_in_pipeline_order() =>
        Assert.Equal(
            "отправлен, просмотрен, приглашение, собеседование, тестовое, оффер, отказ, отозван",
            StatusNames.AllRussian);
}
