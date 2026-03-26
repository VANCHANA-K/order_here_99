using Microsoft.Extensions.Hosting;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;

namespace QrFoodOrdering.Api.Infrastructure;

public static class RuntimeEnvironmentValidator
{
    public static void Validate(
        IHostEnvironment environment,
        RuntimeEnvironmentOptions runtime,
        string? connectionString)
    {
        if (runtime.SeedDemoDataOnStartup && !runtime.ApplyMigrationsOnStartup)
            throw new ConfigurationValidationException(
                ApplicationErrorCodes.ConfigurationInvalid,
                "Invalid runtime config: demo seed requires ApplyMigrationsOnStartup=true."
            );

        if (!environment.IsDevelopment() && runtime.EnableFrontendDevCors)
            throw new ConfigurationValidationException(
                ApplicationErrorCodes.ConfigurationInvalid,
                "Invalid runtime config: frontend dev CORS can only be enabled in Development."
            );

        if (runtime.ApplyMigrationsOnStartup && string.IsNullOrWhiteSpace(connectionString))
            throw new ConfigurationValidationException(
                ApplicationErrorCodes.ConfigurationInvalid,
                "Invalid runtime config: ApplyMigrationsOnStartup requires ConnectionStrings:Default."
            );

        if (environment.IsProduction())
        {
            if (runtime.EnableSwagger)
                throw new ConfigurationValidationException(
                    ApplicationErrorCodes.ConfigurationInvalid,
                    "Invalid runtime config: Swagger must be disabled in Production."
                );

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConfigurationValidationException(
                    ApplicationErrorCodes.ConfigurationInvalid,
                    "Missing ConnectionStrings:Default for Production. Provide it via environment variable or secret store."
                );
        }

        if (environment.IsEnvironment("Test"))
        {
            if (runtime.ApplyMigrationsOnStartup)
                throw new ConfigurationValidationException(
                    ApplicationErrorCodes.ConfigurationInvalid,
                    "Invalid runtime config: startup migrations must be disabled in Test."
                );

            if (runtime.SeedDemoDataOnStartup)
                throw new ConfigurationValidationException(
                    ApplicationErrorCodes.ConfigurationInvalid,
                    "Invalid runtime config: demo seed must be disabled in Test."
                );
        }
    }
}
