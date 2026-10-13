using Microsoft.Extensions.DependencyInjection;
using Soiskatel.Mcp.Storage;
using Soiskatel.Mcp.TextCheck;
using Soiskatel.Mcp.Tools;
using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp;

public static class ServiceRegistration
{
    /// <summary>Логика сервера без MCP: папка поиска, часы, проверка текста, трекер.</summary>
    public static IServiceCollection AddSoiskatelCore(this IServiceCollection services, string searchFolderRoot, TimeProvider timeProvider)
    {
        services.AddSingleton(new SearchFolder(searchFolderRoot));
        services.AddSingleton(timeProvider);
        services.AddSingleton<TextChecker>();
        services.AddSingleton<TrackerService>();
        return services;
    }

    /// <summary>Инструменты MCP поверх логики.</summary>
    public static IMcpServerBuilder AddSoiskatelTools(this IMcpServerBuilder builder) =>
        builder.WithTools<TextCheckTools>().WithTools<TrackerTools>();
}
