namespace Soiskatel.Mcp.Tracker;

/// <summary>Статус отклика. Порядок совпадает с движением по воронке.</summary>
public enum ApplicationStatus
{
    Sent,
    Viewed,
    Invited,
    Interview,
    TestTask,
    Offer,
    Rejected,
    Withdrawn,
}
