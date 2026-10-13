using Soiskatel.Mcp.Storage;

namespace Soiskatel.Mcp.Tracker;

/// <summary>Операции трекера над applications.md в папке поиска.</summary>
public sealed class TrackerService(SearchFolder folder, TimeProvider time)
{
    public const string FileName = "applications.md";

    private DateOnly Today => DateOnly.FromDateTime(time.GetLocalNow().DateTime);

    public AddResult Add(NewApplication input)
    {
        if (string.IsNullOrWhiteSpace(input.Company))
            throw new TrackerException("Укажите компанию.");
        if (string.IsNullOrWhiteSpace(input.Position))
            throw new TrackerException("Укажите вакансию.");

        var doc = Load();
        var date = input.Date ?? Today;
        var application = new JobApplication(
            date,
            input.Company.Trim(),
            input.Position.Trim(),
            input.Platform.Trim(),
            input.Link.Trim(),
            ApplicationStatus.Sent,
            [new HistoryEntry(date, ApplicationStatus.Sent)],
            input.Notes.Trim());

        var earlier = Numbered(doc).Where(n => CompanyName.AreSame(n.Application.Company, application.Company)).ToList();
        var rows = doc.Rows.Append(new ParsedRow(application)).ToList();
        Save(doc with { Rows = rows });

        return new AddResult(new NumberedApplication(rows.Count, application), earlier, doc.Problems);
    }

    public UpdateResult Update(string? company, string? position, int? rowNumber, string newStatus, string? note)
    {
        if (!StatusNames.TryParse(newStatus, out var status))
            throw new TrackerException($"Неизвестный статус «{newStatus}». Допустимые: {StatusNames.AllRussian}.");

        var doc = Load();
        var index = FindIndex(doc, company, position, rowNumber);
        var current = ((ParsedRow)doc.Rows[index]).Application;

        if (!StatusTransitions.IsAllowed(current.Status, status))
            throw new TrackerException(
                $"Нельзя сменить статус «{StatusNames.ToRussian(current.Status)}» на «{StatusNames.ToRussian(status)}». Правила в format.md скилла tracker.");

        var updated = current with
        {
            Status = status,
            History = [.. current.History, new HistoryEntry(Today, status)],
            Notes = AppendNote(current.Notes, note),
        };

        var rows = doc.Rows.ToList();
        rows[index] = new ParsedRow(updated);
        Save(doc with { Rows = rows });

        return new UpdateResult(new NumberedApplication(index + 1, updated), doc.Problems);
    }

    public FindResult FindCompany(string company)
    {
        if (string.IsNullOrWhiteSpace(company))
            throw new TrackerException("Укажите компанию.");

        var doc = Load();
        var all = Numbered(doc).ToList();

        return new FindResult(
            all.Where(n => CompanyName.AreSame(n.Application.Company, company)).ToList(),
            all.Where(n => CompanyName.IsPossibleMatch(n.Application.Company, company)).ToList(),
            doc.Problems);
    }

    public StatsResult Stats(DateOnly? from, DateOnly? to)
    {
        var doc = Load();
        var selected = Numbered(doc)
            .Where(n => from is null || n.Application.Date >= from)
            .Where(n => to is null || n.Application.Date <= to)
            .ToList();

        var funnel = Funnel.Calculate(selected.Select(n => n.Application).ToList(), Today);
        var noResponse = selected.Where(n => funnel.NoResponse.Contains(n.Application)).ToList();

        return new StatsResult(funnel, noResponse, from, to, doc.Problems);
    }

    private static int FindIndex(TrackerDocument doc, string? company, string? position, int? rowNumber)
    {
        if (rowNumber is { } number)
        {
            if (number < 1 || number > doc.Rows.Count)
                throw new TrackerException($"Строки №{number} нет: в таблице {doc.Rows.Count} строк.");
            if (doc.Rows[number - 1] is RawRow raw)
                throw new TrackerException(
                    $"Строка №{number} не разобрана ({raw.Problem}). Исправьте её в applications.md вручную, строка файла {raw.LineNumber}.");

            return number - 1;
        }

        if (string.IsNullOrWhiteSpace(company))
            throw new TrackerException("Укажите компанию или номер строки.");

        var candidates = Numbered(doc)
            .Where(n => CompanyName.AreSame(n.Application.Company, company))
            .Where(n => string.IsNullOrWhiteSpace(position)
                || n.Application.Position.Contains(position.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        return candidates.Count switch
        {
            0 => throw new TrackerException($"Отклик в компанию «{company}» не найден."),
            1 => candidates[0].RowNumber - 1,
            _ => throw new TrackerException(
                "Подходит несколько строк: "
                + string.Join("; ", candidates.Select(c => $"№{c.RowNumber} {c.Application.Company}, {c.Application.Position}"))
                + ". Укажите номер строки."),
        };
    }

    private static IEnumerable<NumberedApplication> Numbered(TrackerDocument doc) =>
        doc.Rows
            .Select((row, i) => (row, number: i + 1))
            .Where(x => x.row is ParsedRow)
            .Select(x => new NumberedApplication(x.number, ((ParsedRow)x.row).Application));

    private static string AppendNote(string notes, string? note) =>
        string.IsNullOrWhiteSpace(note) ? notes
        : string.IsNullOrWhiteSpace(notes) ? note.Trim()
        : $"{notes}; {note.Trim()}";

    private TrackerDocument Load()
    {
        folder.EnsureExists();
        var content = AtomicFile.ReadAllTextOrNull(folder.PathOf(FileName));
        return content is null ? TrackerDocument.Empty : TrackerTable.Parse(content);
    }

    private void Save(TrackerDocument document) =>
        AtomicFile.WriteAllText(folder.PathOf(FileName), TrackerTable.Write(document));
}
