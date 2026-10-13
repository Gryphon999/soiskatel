using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tests.Tracker;

public class TrackerTableTests
{
    private const string Canonical =
        "# Отклики\n" +
        "\n" +
        "| Дата | Компания | Вакансия | Площадка | Ссылка | Статус | История | Заметки |\n" +
        "|---|---|---|---|---|---|---|---|\n" +
        "| 2026-10-13 | Ромашка Софт | Junior C# разработчик | hh.ru | https://example.com/v/1 | приглашение | 2026-10-13 отправлен; 2026-10-15 приглашение | созвон в пятницу |\n" +
        "| 2026-10-14 | Северный Код | Python разработчик | Habr Career |  | отправлен | 2026-10-14 отправлен |  |\n";

    [Fact]
    public void Parse_reads_valid_rows()
    {
        var doc = TrackerTable.Parse(Canonical);
        var apps = doc.Applications.ToList();

        Assert.Equal(2, apps.Count);
        Assert.Empty(doc.Problems);
        Assert.Equal(new DateOnly(2026, 10, 13), apps[0].Date);
        Assert.Equal("Ромашка Софт", apps[0].Company);
        Assert.Equal("Junior C# разработчик", apps[0].Position);
        Assert.Equal("hh.ru", apps[0].Platform);
        Assert.Equal("https://example.com/v/1", apps[0].Link);
        Assert.Equal(ApplicationStatus.Invited, apps[0].Status);
        Assert.Equal(2, apps[0].History.Count);
        Assert.Equal("созвон в пятницу", apps[0].Notes);
        Assert.Equal("", apps[1].Link);
    }

    [Fact]
    public void Write_after_parse_gives_same_text() =>
        Assert.Equal(Canonical, TrackerTable.Write(TrackerTable.Parse(Canonical)));

    [Fact]
    public void Round_trip_keeps_document_equal()
    {
        var doc = TrackerTable.Parse(Canonical);

        var again = TrackerTable.Parse(TrackerTable.Write(doc));

        Assert.Equivalent(doc, again, strict: true);
    }

    [Fact]
    public void Broken_row_is_kept_verbatim_with_file_line_number()
    {
        var content = Canonical + "| 13.10.2026 | Ёлка | QA | hh.ru |  | отправлен | 2026-10-13 отправлен |  |\n";

        var doc = TrackerTable.Parse(content);

        var problem = Assert.Single(doc.Problems);
        Assert.Equal(7, problem.LineNumber);
        Assert.Contains("ГГГГ-ММ-ДД", problem.Problem);
        Assert.Equal(content, TrackerTable.Write(doc));
    }

    [Theory]
    [InlineData("| 2026-10-13 | Ёлка | QA |", "колонок")]
    [InlineData("| 2026-10-13 | Ёлка | QA | hh.ru |  | думаю | 2026-10-13 отправлен |  |", "неизвестный статус")]
    [InlineData("| 2026-10-13 |  | QA | hh.ru |  | отправлен | 2026-10-13 отправлен |  |", "не указана компания")]
    [InlineData("| 2026-10-13 | Ёлка | QA | hh.ru |  | отправлен | вчера |  |", "история")]
    public void Invalid_rows_are_reported(string row, string expectedProblem)
    {
        var doc = TrackerTable.Parse(Canonical + row + "\n");

        Assert.Contains(expectedProblem, Assert.Single(doc.Problems).Problem);
    }

    [Fact]
    public void Pipe_and_newline_in_notes_survive_round_trip()
    {
        var app = new JobApplication(
            new DateOnly(2026, 10, 13), "Ромашка", "C#", "hh.ru", "", ApplicationStatus.Sent,
            [new HistoryEntry(new DateOnly(2026, 10, 13), ApplicationStatus.Sent)],
            "созвон | пятница\nв 15:00");
        var doc = TrackerDocument.Empty with { Rows = [new ParsedRow(app)] };

        var parsed = TrackerTable.Parse(TrackerTable.Write(doc)).Applications.Single();

        Assert.Equal("созвон | пятница в 15:00", parsed.Notes);
    }

    [Fact]
    public void Text_without_table_is_kept_and_table_is_added_after_it()
    {
        var doc = TrackerTable.Parse("Мои заметки о поиске\n");

        Assert.Empty(doc.Rows);
        Assert.StartsWith("Мои заметки о поиске\n" + TrackerTable.HeaderLine, TrackerTable.Write(doc));
    }

    [Fact]
    public void Text_after_table_is_kept()
    {
        var content = Canonical + "\nИтоги недели: 2 отклика\n";

        Assert.Equal(content, TrackerTable.Write(TrackerTable.Parse(content)));
    }

    [Fact]
    public void Windows_line_endings_are_read() =>
        Assert.Equal(2, TrackerTable.Parse(Canonical.Replace("\n", "\r\n")).Applications.Count());

    [Fact]
    public void Empty_content_has_no_rows() =>
        Assert.Empty(TrackerTable.Parse("").Rows);

    [Fact]
    public void Empty_content_has_table_flag_true() =>
        Assert.True(TrackerTable.Parse("").HasTable);

    [Fact]
    public void Whitespace_only_content_has_table_flag_true() =>
        Assert.True(TrackerTable.Parse("   \n\n  ").HasTable);

    [Fact]
    public void Content_with_canonical_header_has_table_flag_true() =>
        Assert.True(TrackerTable.Parse(Canonical).HasTable);

    [Fact]
    public void Text_without_table_has_table_flag_false() =>
        Assert.False(TrackerTable.Parse("Мои заметки о поиске\n").HasTable);

    [Fact]
    public void Windows_line_endings_survive_round_trip()
    {
        var crlf = Canonical.Replace("\n", "\r\n");

        Assert.Equal(crlf, TrackerTable.Write(TrackerTable.Parse(crlf)));
    }

    [Fact]
    public void Leading_bom_does_not_hide_the_table()
    {
        var doc = TrackerTable.Parse("\uFEFF" + Canonical);

        Assert.Equal(2, doc.Applications.Count());
        Assert.Equal(Canonical, TrackerTable.Write(doc));
    }

    [Fact]
    public void Blank_line_inside_table_keeps_the_rest_as_trailing_text()
    {
        var content =
            "| Дата | Компания | Вакансия | Площадка | Ссылка | Статус | История | Заметки |\n" +
            "|---|---|---|---|---|---|---|---|\n" +
            "| 2026-10-13 | Ромашка Софт | Junior C# разработчик | hh.ru | https://example.com/v/1 | приглашение | 2026-10-13 отправлен; 2026-10-15 приглашение | созвон в пятницу |\n" +
            "\n" +
            "| 2026-10-14 | Северный Код | Python разработчик | Habr Career |  | отправлен | 2026-10-14 отправлен |  |\n";

        var doc = TrackerTable.Parse(content);

        Assert.Single(doc.Applications);
        Assert.Equal(content, TrackerTable.Write(doc));
    }

    [Fact]
    public void Backslash_in_cell_survives_round_trip()
    {
        var app = new JobApplication(
            new DateOnly(2026, 10, 13), "Ромашка", "C#", "hh.ru", "", ApplicationStatus.Sent,
            [new HistoryEntry(new DateOnly(2026, 10, 13), ApplicationStatus.Sent)],
            @"C:\work\cv");
        var doc = TrackerDocument.Empty with { Rows = [new ParsedRow(app)] };

        var parsed = TrackerTable.Parse(TrackerTable.Write(doc)).Applications.Single();

        Assert.Equal(@"C:\work\cv", parsed.Notes);
    }
}
