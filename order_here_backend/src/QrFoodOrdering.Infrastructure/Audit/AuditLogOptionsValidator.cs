using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;

namespace QrFoodOrdering.Infrastructure.Audit;

public static class AuditLogOptionsValidator
{
    public static void Validate(AuditLogOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.DirectoryPath))
            throw new ConfigurationValidationException(
                ApplicationErrorCodes.ConfigurationInvalid,
                "Invalid audit log config: AuditLogs:DirectoryPath is required."
            );

        if (options.RetentionDays <= 0)
            throw new ConfigurationValidationException(
                ApplicationErrorCodes.ConfigurationInvalid,
                "Invalid audit log config: AuditLogs:RetentionDays must be greater than 0."
            );

        if (!string.Equals(options.AggregationPeriod, "Daily", StringComparison.OrdinalIgnoreCase))
            throw new ConfigurationValidationException(
                ApplicationErrorCodes.ConfigurationInvalid,
                "Invalid audit log config: AuditLogs:AggregationPeriod must be 'Daily'."
            );
    }
}
