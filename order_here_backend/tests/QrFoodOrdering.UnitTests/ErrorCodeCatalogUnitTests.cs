using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Application.Common.Errors;

namespace QrFoodOrdering.UnitTests;

public sealed class ErrorCodeCatalogUnitTests
{
    [Fact]
    public void All_error_codes_should_be_unique()
    {
        var allCodes = ErrorCodeCatalog.All
            .Concat(
                [
                    ApiErrorCodes.EndpointNotFound,
                    ApiErrorCodes.ServiceUnavailable,
                    ApiErrorCodes.UnexpectedError,
                ]
            )
            .ToList();

        var duplicates = allCodes
            .GroupBy(x => x, StringComparer.Ordinal)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

        Assert.Empty(duplicates);
    }

    [Fact]
    public void All_error_codes_should_use_screaming_snake_case()
    {
        var allCodes = ErrorCodeCatalog.All
            .Concat(
                [
                    ApiErrorCodes.EndpointNotFound,
                    ApiErrorCodes.ServiceUnavailable,
                    ApiErrorCodes.UnexpectedError,
                ]
            );

        foreach (var code in allCodes)
        {
            Assert.Matches("^[A-Z0-9_]+$", code);
        }
    }

    [Fact]
    public void Api_error_semantics_doc_should_reference_standard_catalog_examples()
    {
        var docPath = "/Users/viic/Desktop/order_here/order_here_backend/docs/API_ERROR_SEMANTICS.md";
        var content = File.ReadAllText(docPath);

        Assert.Contains("ORDER_NOT_FOUND", content, StringComparison.Ordinal);
        Assert.Contains("TABLE_NOT_FOUND", content, StringComparison.Ordinal);
        Assert.Contains("QR_NOT_FOUND", content, StringComparison.Ordinal);
        Assert.Contains("ENDPOINT_NOT_FOUND", content, StringComparison.Ordinal);
        Assert.Contains("UNEXPECTED_ERROR", content, StringComparison.Ordinal);
    }
}
