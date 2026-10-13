using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Soiskatel.Mcp.Storage;
using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tools;

[McpServerToolType]
public sealed class TrackerTools(TrackerService tracker)
{
    [McpServerTool(Name = "tracker_add", Title = "Добавить отклик")]
    [Description("Добавляет отправленный отклик в applications.md в папке поиска: статус «отправлен», дата сегодня, если не указана. Вызывай только после того, как человек подтвердил, что отклик отправлен.")]
    public string Add(
        [Description("Компания")] string company,
        [Description("Вакансия (должность)")] string position,
        [Description("Площадка: hh.ru, Habr Career, GeekJob, staff.am и т.п.")] string platform = "",
        [Description("Ссылка на вакансию")] string link = "",
        [Description("Заметки")] string notes = "",
        [Description("Дата отклика ГГГГ-ММ-ДД, если не сегодня")] string? date = null) =>
        Run(() => ToolText.Format(tracker.Add(new NewApplication(company, position, platform, link, notes, ParseDate(date)))));

    [McpServerTool(Name = "tracker_update", Title = "Сменить статус отклика")]
    [Description("Меняет статус отклика и дописывает историю. Отклик ищется по компании (и части названия вакансии, если откликов в компанию несколько) или по номеру строки таблицы. Статусы: отправлен, просмотрен, приглашение, собеседование, тестовое, оффер, отказ, отозван.")]
    public string Update(
        [Description("Новый статус")] string status,
        [Description("Компания")] string? company = null,
        [Description("Часть названия вакансии")] string? position = null,
        [Description("Номер строки таблицы, 1 = первая строка")] int? row = null,
        [Description("Заметка, будет дописана к заметкам")] string? note = null) =>
        Run(() => ToolText.Format(tracker.Update(company, position, row, status, note)));

    [McpServerTool(Name = "tracker_find_company", Title = "Отклики в компанию", ReadOnly = true)]
    [Description("Показывает все отклики в компанию из applications.md и возможные совпадения по похожим названиям.")]
    public string FindCompany([Description("Название компании")] string company) =>
        Run(() => ToolText.Format(tracker.FindCompany(company), company));

    [McpServerTool(Name = "tracker_stats", Title = "Воронка откликов", ReadOnly = true)]
    [Description("Считает воронку откликов: отправлено, ответили, собеседования, офферы с процентами и отклики без ответа 7 дней и больше.")]
    public string Stats(
        [Description("С даты ГГГГ-ММ-ДД. Необязательно.")] string? from = null,
        [Description("По дату ГГГГ-ММ-ДД. Необязательно.")] string? to = null) =>
        Run(() => ToolText.Format(tracker.Stats(ParseDate(from), ParseDate(to))));

    private static DateOnly? ParseDate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (IsoDate.TryParse(text, out var date))
            return date;

        throw new TrackerException($"Дата «{text}» не в формате ГГГГ-ММ-ДД.");
    }

    /// <summary>Ошибки логики превращаем в McpException: SDK вернёт их текст как ошибку инструмента.</summary>
    private static string Run(Func<string> action)
    {
        try
        {
            return action();
        }
        catch (TrackerException ex)
        {
            throw new McpException(ex.Message);
        }
        catch (StorageException ex)
        {
            throw new McpException(ex.Message);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new McpException("Не удалось прочитать или записать applications.md: файл занят другой программой или нет доступа. Закройте файл и повторите.");
        }
    }
}
