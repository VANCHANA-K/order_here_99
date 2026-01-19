using System.Text.Json;
using QrFoodOrdering.Domain.Audit;

namespace QrFoodOrdering.Infrastructure.Audit;

public sealed class FileAuditLogWriter : IAuditLogWriter
{
    private readonly string _filePath;

    public FileAuditLogWriter()
    {
        Directory.CreateDirectory("data/audit");
        _filePath = Path.Combine("data/audit", "audit.log");
    }

    public async Task WriteAsync(AuditLog log)
    {
        var line = JsonSerializer.Serialize(log);
        await File.AppendAllTextAsync(_filePath, line + Environment.NewLine);
    }
}
