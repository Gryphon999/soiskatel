namespace Soiskatel.Mcp.TextCheck;

public sealed record PlatformLimit(string Name, int MaxLength);

/// <summary>Лимиты длины письма. staff.am режет письмо на 500 символах; длинные письма в остальных местах не дочитывают.</summary>
public static class PlatformLimits
{
    public static PlatformLimit For(string? platform) =>
        platform is not null && platform.Contains("staff", StringComparison.OrdinalIgnoreCase)
            ? new PlatformLimit("staff.am", 500)
            : new PlatformLimit("обычное письмо", 2000);
}
