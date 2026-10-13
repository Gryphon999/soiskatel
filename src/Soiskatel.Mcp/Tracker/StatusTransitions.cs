namespace Soiskatel.Mcp.Tracker;

/// <summary>Какие смены статуса разрешены. Те же правила описаны для людей в format.md скилла tracker.</summary>
public static class StatusTransitions
{
    /// <summary>
    /// Правила: вперёд по цепочке можно, назад нельзя, тот же статус ставить нельзя;
    /// «собеседование» и «тестовое» на одном уровне, между ними можно в обе стороны;
    /// в «отказ» и «отозван» можно из любого статуса, кроме самих «отказ» и «отозван»;
    /// из «отказ» можно только в «приглашение»; из «отозван» и «оффер» больше никуда, кроме сказанного выше.
    /// </summary>
    public static bool IsAllowed(ApplicationStatus from, ApplicationStatus to)
    {
        if (from == to) return false;
        if (to is ApplicationStatus.Rejected or ApplicationStatus.Withdrawn)
            return from is not (ApplicationStatus.Rejected or ApplicationStatus.Withdrawn);
        if (from == ApplicationStatus.Rejected) return to == ApplicationStatus.Invited;
        if (from is ApplicationStatus.Withdrawn or ApplicationStatus.Offer) return false;
        if ((from is ApplicationStatus.Interview or ApplicationStatus.TestTask) && (to is ApplicationStatus.Interview or ApplicationStatus.TestTask)) return true;
        return Rank(to) > Rank(from);
    }

    /// <summary>Уровень в воронке: чем дальше от отправки, тем больше. Для «отказ» и «отозван» уровня нет.</summary>
    private static int Rank(ApplicationStatus status) => status switch
    {
        ApplicationStatus.Sent => 0,
        ApplicationStatus.Viewed => 1,
        ApplicationStatus.Invited => 2,
        ApplicationStatus.Interview or ApplicationStatus.TestTask => 3,
        ApplicationStatus.Offer => 4,
        _ => -1,
    };
}
