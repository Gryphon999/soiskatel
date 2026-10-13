using Microsoft.Extensions.Time.Testing;
using Soiskatel.Mcp.Storage;
using Soiskatel.Mcp.Tests.TestSupport;
using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tests.Tracker;

public class TrackerServiceTests
{
    private static FakeTimeProvider Clock() => new(new DateTimeOffset(2026, 10, 13, 9, 0, 0, TimeSpan.Zero));

    private static NewApplication Romashka(string position = "Junior C# разработчик") =>
        new("ООО «Ромашка Софт»", position, "hh.ru", "https://example.com/v/1", "", Date: null);

    [Fact]
    public void Add_creates_file_with_header_and_row()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());

        var result = service.Add(Romashka());

        var content = File.ReadAllText(temp.File(TrackerService.FileName));
        Assert.Contains(TrackerTable.HeaderLine, content);
        Assert.Contains("| 2026-10-13 | ООО «Ромашка Софт» | Junior C# разработчик | hh.ru | https://example.com/v/1 | отправлен | 2026-10-13 отправлен |  |", content);
        Assert.Equal(1, result.Added.RowNumber);
        Assert.Empty(result.EarlierInSameCompany);
        Assert.False(File.Exists(temp.File(TrackerService.FileName + ".bak")));
    }

    [Fact]
    public void Second_add_keeps_backup_and_reports_earlier_application_to_same_company()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());
        service.Add(Romashka());

        var result = service.Add(Romashka("Middle C# разработчик") with { Company = "Ромашка Софт" });

        Assert.Equal(2, result.Added.RowNumber);
        Assert.Equal(1, Assert.Single(result.EarlierInSameCompany).RowNumber);
        Assert.True(File.Exists(temp.File(TrackerService.FileName + ".bak")));
    }

    [Fact]
    public void Add_requires_company()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());

        var ex = Assert.Throws<TrackerException>(() => service.Add(Romashka() with { Company = " " }));

        Assert.Contains("компанию", ex.Message);
    }

    [Fact]
    public void Update_changes_status_and_appends_history_with_today()
    {
        using var temp = new TempFolder();
        var clock = Clock();
        var service = new TrackerService(new SearchFolder(temp.Root), clock);
        service.Add(Romashka());
        clock.Advance(TimeSpan.FromDays(2));

        var result = service.Update("Ромашка Софт", position: null, rowNumber: null, "приглашение", "созвон в пятницу");

        Assert.Equal(ApplicationStatus.Invited, result.Updated.Application.Status);
        Assert.Equal("2026-10-13 отправлен; 2026-10-15 приглашение", HistoryText.Format(result.Updated.Application.History));
        Assert.Equal("созвон в пятницу", result.Updated.Application.Notes);
    }

    [Fact]
    public void Update_rejects_forbidden_transition()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());
        service.Add(Romashka());
        service.Update("Ромашка Софт", null, null, "собеседование", null);

        var ex = Assert.Throws<TrackerException>(() => service.Update("Ромашка Софт", null, null, "отправлен", null));

        Assert.Contains("Нельзя сменить статус", ex.Message);
    }

    [Fact]
    public void Update_with_unknown_status_lists_allowed_ones()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());
        service.Add(Romashka());

        var ex = Assert.Throws<TrackerException>(() => service.Update("Ромашка Софт", null, null, "думаю", null));

        Assert.Contains(StatusNames.AllRussian, ex.Message);
    }

    [Fact]
    public void Update_by_row_number()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());
        service.Add(Romashka());
        service.Add(Romashka("Middle C# разработчик"));

        var result = service.Update(company: null, position: null, rowNumber: 2, "просмотрен", null);

        Assert.Equal("Middle C# разработчик", result.Updated.Application.Position);
    }

    [Fact]
    public void Update_with_ambiguous_company_lists_rows()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());
        service.Add(Romashka());
        service.Add(Romashka("Middle C# разработчик"));

        var ex = Assert.Throws<TrackerException>(() => service.Update("Ромашка Софт", null, null, "отказ", null));

        Assert.Contains("№1", ex.Message);
        Assert.Contains("№2", ex.Message);
    }

    [Fact]
    public void Broken_rows_survive_add_and_cannot_be_updated()
    {
        using var temp = new TempFolder();
        var broken = "| 13.10.2026 | Ёлка | QA | hh.ru |  | отправлен | 2026-10-13 отправлен |  |";
        File.WriteAllText(
            temp.File(TrackerService.FileName),
            "# Отклики\n\n" + TrackerTable.HeaderLine + "\n" + TrackerTable.SeparatorLine + "\n" + broken + "\n");
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());

        var added = service.Add(Romashka());

        Assert.Single(added.Problems);
        Assert.Contains(broken, File.ReadAllText(temp.File(TrackerService.FileName)));
        var ex = Assert.Throws<TrackerException>(() => service.Update(null, null, 1, "отказ", null));
        Assert.Contains("не разобрана", ex.Message);
    }

    [Fact]
    public void FindCompany_returns_matches_and_possible_matches()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());
        service.Add(new NewApplication("Яндекс", "C# разработчик", "hh.ru", "", "", null));
        service.Add(new NewApplication("Яндекс Маркет", "C# разработчик", "hh.ru", "", "", null));

        var result = service.FindCompany("ООО Яндекс");

        Assert.Equal(1, Assert.Single(result.Matches).RowNumber);
        Assert.Equal(2, Assert.Single(result.PossibleMatches).RowNumber);
    }

    [Fact]
    public void Stats_counts_funnel_and_no_response()
    {
        using var temp = new TempFolder();
        var clock = Clock();
        var service = new TrackerService(new SearchFolder(temp.Root), clock);
        service.Add(Romashka());
        service.Add(new NewApplication("Северный Код", "Python разработчик", "Habr Career", "", "", null));
        service.Update("Ромашка Софт", null, null, "приглашение", null);
        clock.Advance(TimeSpan.FromDays(8));

        var result = service.Stats(from: null, to: null);

        Assert.Equal(2, result.Funnel.Sent);
        Assert.Equal(1, result.Funnel.Responded);
        Assert.Equal(2, Assert.Single(result.NoResponse).RowNumber);
    }

    [Fact]
    public void Stats_on_missing_file_returns_zeros_and_creates_nothing()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());

        var result = service.Stats(null, null);

        Assert.Equal(0, result.Funnel.Sent);
        Assert.False(File.Exists(temp.File(TrackerService.FileName)));
    }

    [Fact]
    public void Parallel_adds_do_not_lose_updates()
    {
        using var temp = new TempFolder();
        var service = new TrackerService(new SearchFolder(temp.Root), Clock());

        Parallel.For(0, 20, i => service.Add(Romashka($"Позиция {i}")));

        var content = File.ReadAllText(temp.File(TrackerService.FileName));
        var doc = TrackerTable.Parse(content);
        Assert.Equal(20, doc.Applications.Count());
    }

    [Fact]
    public void Missing_search_folder_gives_clear_error()
    {
        var missing = Path.Combine(Path.GetTempPath(), "soiskatel-tests", "missing-" + Guid.NewGuid().ToString("N"));
        var service = new TrackerService(new SearchFolder(missing), Clock());

        Assert.Throws<StorageException>(() => service.Stats(null, null));
    }
}
