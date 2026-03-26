using Microsoft.Extensions.Options;
using QrFoodOrdering.Domain.Audit;
using QrFoodOrdering.Infrastructure.Audit;

namespace QrFoodOrdering.UnitTests;

public sealed class FileAuditLogWriterUnitTests
{
    [Fact]
    public async Task WriteAsync_should_aggregate_by_day_and_delete_expired_files()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"audit-writer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var expiredFile = Path.Combine(tempDir, "audit-20000101.log");
            await File.WriteAllTextAsync(expiredFile, "old");
            File.SetLastWriteTimeUtc(expiredFile, DateTime.UtcNow.AddDays(-10));

            var options = Options.Create(
                new AuditLogOptions
                {
                    DirectoryPath = tempDir,
                    RetentionDays = 3,
                    AggregationPeriod = "Daily"
                }
            );

            var writer = new FileAuditLogWriter(options);
            var log = new AuditLog("ORDER_CREATED", "Order", Guid.NewGuid(), "meta", "trace-test");

            await writer.WriteAsync(log);

            var currentFile = Path.Combine(tempDir, $"audit-{DateTime.UtcNow:yyyyMMdd}.log");
            Assert.True(File.Exists(currentFile));
            Assert.False(File.Exists(expiredFile));

            var content = await File.ReadAllTextAsync(currentFile);
            Assert.Contains("ORDER_CREATED", content, StringComparison.Ordinal);
            Assert.Contains("trace-test", content, StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }
}
