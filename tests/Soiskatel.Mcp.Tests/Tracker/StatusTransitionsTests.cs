using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tests.Tracker;

public class StatusTransitionsTests
{
    [Theory]
    [InlineData(ApplicationStatus.Sent, ApplicationStatus.Viewed)]
    [InlineData(ApplicationStatus.Sent, ApplicationStatus.Invited)]
    [InlineData(ApplicationStatus.Viewed, ApplicationStatus.Invited)]
    [InlineData(ApplicationStatus.Invited, ApplicationStatus.Interview)]
    [InlineData(ApplicationStatus.Invited, ApplicationStatus.TestTask)]
    [InlineData(ApplicationStatus.Interview, ApplicationStatus.TestTask)]
    [InlineData(ApplicationStatus.TestTask, ApplicationStatus.Interview)]
    [InlineData(ApplicationStatus.Interview, ApplicationStatus.Offer)]
    [InlineData(ApplicationStatus.Sent, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Interview, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Offer, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Offer, ApplicationStatus.Withdrawn)]
    [InlineData(ApplicationStatus.Viewed, ApplicationStatus.Withdrawn)]
    public void Allowed(ApplicationStatus from, ApplicationStatus to) =>
        Assert.True(StatusTransitions.IsAllowed(from, to));

    [Theory]
    [InlineData(ApplicationStatus.Sent, ApplicationStatus.Sent)]
    [InlineData(ApplicationStatus.Interview, ApplicationStatus.Invited)]
    [InlineData(ApplicationStatus.Viewed, ApplicationStatus.Sent)]
    [InlineData(ApplicationStatus.Offer, ApplicationStatus.Interview)]
    [InlineData(ApplicationStatus.Withdrawn, ApplicationStatus.Invited)]
    [InlineData(ApplicationStatus.Withdrawn, ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.Withdrawn)]
    [InlineData(ApplicationStatus.Rejected, ApplicationStatus.Interview)]
    public void Forbidden(ApplicationStatus from, ApplicationStatus to) =>
        Assert.False(StatusTransitions.IsAllowed(from, to));

    // Решение автора. По умолчанию из спецификации: работодатели иногда возвращаются после отказа.
    [Fact]
    public void Rejected_can_return_to_invited() =>
        Assert.True(StatusTransitions.IsAllowed(ApplicationStatus.Rejected, ApplicationStatus.Invited));
}
