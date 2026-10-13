using System.ComponentModel;
using ModelContextProtocol.Server;
using Soiskatel.Mcp.TextCheck;

namespace Soiskatel.Mcp.Tools;

[McpServerToolType]
public sealed class TextCheckTools(TextChecker checker)
{
    [McpServerTool(Name = "text_check", Title = "Проверка письма", ReadOnly = true)]
    [Description("Проверяет текст сопроводительного письма по правилам рунета: убирает невидимые символы, неразрывные пробелы, длинные тире, кавычки-лапки и многоточие; предупреждает о стрелках, слове «участие», отсутствии цифр и превышении длины площадки. Файлы не меняет.")]
    public string TextCheck(
        [Description("Текст письма целиком")] string text,
        [Description("Площадка отклика, например hh.ru или staff.am. Необязательно.")] string? platform = null) =>
        ToolText.Format(checker.Check(text, platform));
}
