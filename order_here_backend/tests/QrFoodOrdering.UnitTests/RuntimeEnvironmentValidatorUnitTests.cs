using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using QrFoodOrdering.Api.Infrastructure;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;

namespace QrFoodOrdering.UnitTests;

public sealed class RuntimeEnvironmentValidatorUnitTests
{
    [Fact]
    public void Validate_should_allow_development_defaults()
    {
        var env = new StubHostEnvironment(Environments.Development);
        var runtime = new RuntimeEnvironmentOptions
        {
            EnableSwagger = true,
            EnableFrontendDevCors = true,
            ApplyMigrationsOnStartup = true,
            SeedDemoDataOnStartup = true
        };

        RuntimeEnvironmentValidator.Validate(env, runtime, "Data Source=qrfood.dev.db");
    }

    [Fact]
    public void Validate_should_throw_when_seed_enabled_without_migrations()
    {
        var env = new StubHostEnvironment(Environments.Development);
        var runtime = new RuntimeEnvironmentOptions
        {
            ApplyMigrationsOnStartup = false,
            SeedDemoDataOnStartup = true
        };

        var ex = Assert.Throws<ConfigurationValidationException>(() =>
            RuntimeEnvironmentValidator.Validate(env, runtime, "Data Source=qrfood.dev.db")
        );

        Assert.Equal(ApplicationErrorCodes.ConfigurationInvalid, ex.ErrorCode);
        Assert.Contains("demo seed requires", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_should_throw_when_dev_cors_enabled_outside_development()
    {
        var env = new StubHostEnvironment(Environments.Production);
        var runtime = new RuntimeEnvironmentOptions
        {
            EnableFrontendDevCors = true
        };

        var ex = Assert.Throws<ConfigurationValidationException>(() =>
            RuntimeEnvironmentValidator.Validate(env, runtime, "Data Source=prod.db")
        );

        Assert.Equal(ApplicationErrorCodes.ConfigurationInvalid, ex.ErrorCode);
        Assert.Contains("frontend dev CORS", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_should_throw_when_production_connection_string_is_missing()
    {
        var env = new StubHostEnvironment(Environments.Production);
        var runtime = new RuntimeEnvironmentOptions();

        var ex = Assert.Throws<ConfigurationValidationException>(() =>
            RuntimeEnvironmentValidator.Validate(env, runtime, null)
        );

        Assert.Equal(ApplicationErrorCodes.ConfigurationInvalid, ex.ErrorCode);
        Assert.Contains("Missing ConnectionStrings:Default", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_should_throw_when_production_swagger_is_enabled()
    {
        var env = new StubHostEnvironment(Environments.Production);
        var runtime = new RuntimeEnvironmentOptions
        {
            EnableSwagger = true
        };

        var ex = Assert.Throws<ConfigurationValidationException>(() =>
            RuntimeEnvironmentValidator.Validate(env, runtime, "Data Source=prod.db")
        );

        Assert.Equal(ApplicationErrorCodes.ConfigurationInvalid, ex.ErrorCode);
        Assert.Contains("Swagger must be disabled", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_should_throw_when_test_enables_startup_migrations()
    {
        var env = new StubHostEnvironment("Test");
        var runtime = new RuntimeEnvironmentOptions
        {
            ApplyMigrationsOnStartup = true
        };

        var ex = Assert.Throws<ConfigurationValidationException>(() =>
            RuntimeEnvironmentValidator.Validate(env, runtime, "Data Source=test.db")
        );

        Assert.Equal(ApplicationErrorCodes.ConfigurationInvalid, ex.ErrorCode);
        Assert.Contains("startup migrations must be disabled in Test", ex.Message, StringComparison.Ordinal);
    }

    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public StubHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "QrFoodOrdering.Tests";
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
