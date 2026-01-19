using QrFoodOrdering.Domain.Audit;

namespace QrFoodOrdering.Infrastructure.Audit;

public interface IAuditLogWriter
{
    Task WriteAsync(AuditLog log);
}
