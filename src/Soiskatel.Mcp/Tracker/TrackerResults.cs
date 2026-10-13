namespace Soiskatel.Mcp.Tracker;

public sealed record NewApplication(string Company, string Position, string Platform, string Link, string Notes, DateOnly? Date);

/// <summary>Отклик с номером строки таблицы: 1 означает первую строку.</summary>
public sealed record NumberedApplication(int RowNumber, JobApplication Application);

public sealed record AddResult(
    NumberedApplication Added,
    IReadOnlyList<NumberedApplication> EarlierInSameCompany,
    IReadOnlyList<RawRow> Problems);

public sealed record UpdateResult(NumberedApplication Updated, IReadOnlyList<RawRow> Problems);

public sealed record FindResult(
    IReadOnlyList<NumberedApplication> Matches,
    IReadOnlyList<NumberedApplication> PossibleMatches,
    IReadOnlyList<RawRow> Problems);

public sealed record StatsResult(
    FunnelStats Funnel,
    IReadOnlyList<NumberedApplication> NoResponse,
    DateOnly? From,
    DateOnly? To,
    IReadOnlyList<RawRow> Problems);
