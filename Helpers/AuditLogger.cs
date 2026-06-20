using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace RehabCenterApp.Helpers;

public static class AuditLogger
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RehabCenterApp", "audit.log");

    static AuditLogger()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
    }

    public static async Task LogAsync(string action, string entity, string? details = null)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {action} | Entity: {entity} | {details ?? ""}{Environment.NewLine}";
        await File.AppendAllTextAsync(LogPath, line);
    }

    public static async Task<string[]> GetRecentLogsAsync(int count = 100)
    {
        if (!File.Exists(LogPath)) return Array.Empty<string>();
        var lines = await File.ReadAllLinesAsync(LogPath);
        return lines.TakeLast(count).Reverse().ToArray();
    }
}