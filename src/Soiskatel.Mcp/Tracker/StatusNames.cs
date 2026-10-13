namespace Soiskatel.Mcp.Tracker;

/// <summary>Русские названия статусов, как они пишутся в applications.md.</summary>
public static class StatusNames
{
    private static readonly (ApplicationStatus Status, string Name)[] Table =
    [
        (ApplicationStatus.Sent, "отправлен"),
        (ApplicationStatus.Viewed, "просмотрен"),
        (ApplicationStatus.Invited, "приглашение"),
        (ApplicationStatus.Interview, "собеседование"),
        (ApplicationStatus.TestTask, "тестовое"),
        (ApplicationStatus.Offer, "оффер"),
        (ApplicationStatus.Rejected, "отказ"),
        (ApplicationStatus.Withdrawn, "отозван"),
    ];

    public static string AllRussian { get; } = string.Join(", ", Table.Select(t => t.Name));

    public static string ToRussian(ApplicationStatus status) => Table.Single(t => t.Status == status).Name;

    public static bool TryParse(string? text, out ApplicationStatus status)
    {
        var normalized = text?.Trim().ToLowerInvariant().Replace('ё', 'е');
        foreach (var (candidate, name) in Table)
        {
            if (name == normalized)
            {
                status = candidate;
                return true;
            }
        }

        status = default;
        return false;
    }
}
