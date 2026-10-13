namespace Soiskatel.Mcp.Tracker;

/// <summary>Итог воронки откликов.</summary>
public sealed record FunnelStats(int Sent, int Responded, int Interviews, int Offers, IReadOnlyList<JobApplication> NoResponse);

/// <summary>Воронка. Те же правила описаны для людей в format.md скилла tracker, раздел «Воронка».</summary>
public static class Funnel
{
    public const int NoResponseDays = 7;

    /// <summary>Статусы, которые считаются ответом работодателя. Решение автора: отказ тоже ответ.</summary>
    public static IReadOnlyList<ApplicationStatus> ResponseStatuses { get; } =
    [
        ApplicationStatus.Invited,
        ApplicationStatus.Interview,
        ApplicationStatus.TestTask,
        ApplicationStatus.Offer,
        ApplicationStatus.Rejected,
    ];

    /// <summary>
    /// Sent: все отклики. Responded: в истории есть статус из ResponseStatuses.
    /// Interviews: в истории есть «собеседование». Offers: в истории есть «оффер».
    /// NoResponse: статус «отправлен» или «просмотрен», и с последней записи истории прошло NoResponseDays дней или больше.
    /// </summary>
    public static FunnelStats Calculate(IReadOnlyCollection<JobApplication> applications, DateOnly today)
    {
        var responded = applications.Count(a => a.History.Any(h => ResponseStatuses.Contains(h.Status)));
        var interviews = applications.Count(a => a.Reached(ApplicationStatus.Interview));
        var offers = applications.Count(a => a.Reached(ApplicationStatus.Offer));
        var noResponse = applications
            .Where(a => a.Status is ApplicationStatus.Sent or ApplicationStatus.Viewed)
            .Where(a => today.DayNumber - a.LastChange.DayNumber >= NoResponseDays)
            .ToList();
        return new FunnelStats(applications.Count, responded, interviews, offers, noResponse);
    }
}
