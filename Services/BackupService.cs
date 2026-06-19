using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace RehabCenterApp.Services;

public class BackupService
{
    private readonly AppDbContext _context;

    public BackupService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateBackupAsync()
    {
        var dbPath = _context.Database.GetDbConnection().DataSource;
        var backupDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RehabCenterApp", "Backups");

        Directory.CreateDirectory(backupDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFile = Path.Combine(backupDir, $"backup_{timestamp}.zip");

        // Ensure connection is closed before backup
        await _context.Database.CloseConnectionAsync();

        try
        {
            File.Copy(dbPath, Path.Combine(backupDir, $"data_{timestamp}.db"), true);

            using (var archive = ZipFile.Open(backupFile, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(
                    Path.Combine(backupDir, $"data_{timestamp}.db"),
                    "data.db");
            }

            File.Delete(Path.Combine(backupDir, $"data_{timestamp}.db"));

            // Clean old backups (keep last 30)
            CleanupOldBackups(backupDir);
        }
        finally
        {
            await _context.Database.OpenConnectionAsync();
        }

        return backupFile;
    }

    public async Task RestoreBackupAsync(string backupFilePath)
    {
        var dbPath = _context.Database.GetDbConnection().DataSource;

        await _context.Database.CloseConnectionAsync();

        try
        {
            using (var archive = ZipFile.OpenRead(backupFilePath))
            {
                var entry = archive.GetEntry("data.db");
                if (entry != null)
                {
                    entry.ExtractToFile(dbPath, true);
                }
            }
        }
        finally
        {
            await _context.Database.OpenConnectionAsync();
        }
    }

    private void CleanupOldBackups(string backupDir)
    {
        var files = Directory.GetFiles(backupDir, "backup_*.zip")
            .OrderByDescending(f => f)
            .Skip(30);

        foreach (var file in files)
        {
            try { File.Delete(file); } catch { }
        }
    }
}