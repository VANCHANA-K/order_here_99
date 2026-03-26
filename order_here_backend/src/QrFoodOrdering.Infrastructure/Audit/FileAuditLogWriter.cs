using System.Text.Json;
using Microsoft.Extensions.Options;
using QrFoodOrdering.Domain.Audit;

namespace QrFoodOrdering.Infrastructure.Audit;

public sealed class FileAuditLogWriter : IAuditLogWriter
{
    private readonly AuditLogOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public FileAuditLogWriter(IOptions<AuditLogOptions> options)
    {
        _options = options.Value;
        Directory.CreateDirectory(_options.DirectoryPath);
    }

    public async Task WriteAsync(AuditLog log)
    {
        await _gate.WaitAsync();
        try
        {
            CleanupExpiredFiles();

            var filePath = GetCurrentFilePath();
            var line = JsonSerializer.Serialize(log);
            await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
        }
        finally
        {
            _gate.Release();
        }
    }

    private string GetCurrentFilePath()
    {
        var stamp = DateTime.UtcNow.ToString("yyyyMMdd");
        return Path.Combine(_options.DirectoryPath, $"audit-{stamp}.log");
    }

    private void CleanupExpiredFiles()
    {
        var cutoffUtc = DateTime.UtcNow.Date.AddDays(-_options.RetentionDays);

        foreach (var path in Directory.GetFiles(_options.DirectoryPath, "audit-*.log"))
        {
            var lastWriteUtc = File.GetLastWriteTimeUtc(path);
            if (lastWriteUtc < cutoffUtc)
                File.Delete(path);
        }
    }
}
