using Microsoft.AspNetCore.Mvc.ModelBinding;
using QrFoodOrdering.Api.Contracts.Orders;
using QrFoodOrdering.Api.Contracts.Tables;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Validation;
using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Api.Validation;

public static class ModelValidationErrorMapper
{
    private static readonly ValidationRule[] Rules =
    [
        new(
            nameof(AddItemRequest.ProductName),
            nameof(AddItemRequest.ProductName).ToLowerInvariant(),
            ApplicationErrorCodes.ProductNameRequired,
            RequestValidationMessages.ProductNameRequired
        ),
        new(
            nameof(AddItemRequest.Quantity),
            nameof(AddItemRequest.Quantity).ToLowerInvariant(),
            ApplicationErrorCodes.InvalidQuantity,
            RequestValidationMessages.QuantityMustBeGreaterThanZero
        ),
        new(
            nameof(AddItemRequest.UnitPrice),
            nameof(AddItemRequest.UnitPrice).ToLowerInvariant(),
            ApplicationErrorCodes.UnitPriceInvalid,
            RequestValidationMessages.UnitPriceMustBePositive
        ),
        new(
            nameof(CreateOrderRequest.TableId),
            nameof(CreateOrderRequest.TableId).ToLowerInvariant(),
            ApplicationErrorCodes.TableIdRequired,
            RequestValidationMessages.TableIdRequired
        ),
        new(
            nameof(CreateOrderViaQrRequest.TableId),
            nameof(CreateOrderViaQrRequest.TableId).ToLowerInvariant(),
            ApplicationErrorCodes.TableIdRequired,
            RequestValidationMessages.TableIdRequired
        ),
        new(
            nameof(CreateOrderViaQrItemRequest.MenuItemId),
            nameof(CreateOrderViaQrItemRequest.MenuItemId).ToLowerInvariant(),
            ApplicationErrorCodes.MenuItemIdRequired,
            RequestValidationMessages.MenuItemIdRequired
        ),
        new(
            nameof(CreateOrderViaQrItemRequest.Quantity),
            nameof(CreateOrderViaQrItemRequest.Quantity).ToLowerInvariant(),
            ApplicationErrorCodes.InvalidQuantity,
            RequestValidationMessages.QuantityMustBeGreaterThanZero
        ),
        new(
            nameof(CreateOrderViaQrRequest.Items),
            nameof(CreateOrderViaQrRequest.Items).ToLowerInvariant(),
            ApplicationErrorCodes.EmptyItems,
            RequestValidationMessages.EmptyItems
        ),
        new(
            nameof(CreateTableRequest.Code),
            nameof(CreateTableRequest.Code).ToLowerInvariant(),
            DomainErrorCodes.TableCodeRequired,
            RequestValidationMessages.TableCodeRequired
        ),
    ];

    public static (string ErrorCode, string Message) Map(ModelStateDictionary modelState)
    {
        List<(string Key, string Message)> errors = modelState
            .Where(x => x.Value is { Errors.Count: > 0 })
            .Select(x => (x.Key, x.Value!.Errors[0].ErrorMessage))
            .ToList();

        if (errors.Count == 0)
            return (
                ApplicationErrorCodes.RequestBodyRequired,
                RequestValidationMessages.RequestBodyRequired
            );

        foreach (var error in errors)
        {
            if (
                string.IsNullOrEmpty(error.Key)
                && error.Message.Contains(
                    "A non-empty request body is required",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return (
                    ApplicationErrorCodes.RequestBodyRequired,
                    RequestValidationMessages.RequestBodyRequired
                );
            }
        }

        foreach (var error in errors)
        {
            if (!IsJsonParsingError(error))
                continue;

            if (ContainsPattern(error.Key, nameof(AddItemRequest.Quantity)) || ContainsPattern(error.Message, nameof(AddItemRequest.Quantity)) || ContainsPattern(error.Key, nameof(CreateOrderViaQrItemRequest.Quantity)) || ContainsPattern(error.Message, nameof(CreateOrderViaQrItemRequest.Quantity)))
            {
                return (
                    ApplicationErrorCodes.InvalidQuantity,
                    RequestValidationMessages.QuantityMustBeGreaterThanZero
                );
            }

            if (ContainsPattern(error.Key, nameof(AddItemRequest.UnitPrice)) || ContainsPattern(error.Message, nameof(AddItemRequest.UnitPrice)))
            {
                return (
                    ApplicationErrorCodes.UnitPriceInvalid,
                    RequestValidationMessages.UnitPriceMustBePositive
                );
            }

            if (
                ContainsPattern(error.Key, nameof(CreateOrderRequest.TableId))
                || ContainsPattern(error.Key, nameof(CreateOrderViaQrRequest.TableId))
                || ContainsPattern(error.Message, nameof(CreateOrderRequest.TableId))
                || ContainsPattern(error.Message, nameof(CreateOrderViaQrRequest.TableId))
            )
            {
                return (ApplicationErrorCodes.TableIdInvalid, RequestValidationMessages.TableIdInvalid);
            }

            if (
                ContainsPattern(error.Key, nameof(CreateOrderViaQrItemRequest.MenuItemId))
                || ContainsPattern(error.Message, nameof(CreateOrderViaQrItemRequest.MenuItemId))
            )
            {
                return (
                    ApplicationErrorCodes.MenuItemIdInvalid,
                    RequestValidationMessages.MenuItemIdInvalid
                );
            }

            return (ApplicationErrorCodes.InvalidJson, RequestValidationMessages.InvalidJson);
        }

        foreach (var rule in Rules)
        {
            if (
                errors.Any(x =>
                    ContainsPattern(x.Key, rule.FieldName)
                    || ContainsPattern(x.Key, rule.JsonFieldName)
                    || ContainsPattern(x.Message, rule.FieldName)
                    || ContainsPattern(x.Message, rule.JsonFieldName)
                )
            )
                return (rule.ErrorCode, rule.Message);
        }

        var firstError = errors[0];

        return (
            ApplicationErrorCodes.InvalidRequest,
            string.IsNullOrWhiteSpace(firstError.Message)
                ? RequestValidationMessages.InvalidRequest
                : firstError.Message
        );
    }

    private static bool ContainsPattern(string? value, string pattern) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Contains(pattern, StringComparison.OrdinalIgnoreCase);

    private static bool IsJsonParsingError((string Key, string Message) error) =>
        error.Key.StartsWith("$", StringComparison.Ordinal)
        || error.Message.Contains("could not be converted", StringComparison.OrdinalIgnoreCase)
        || error.Message.Contains("is not valid JSON", StringComparison.OrdinalIgnoreCase);

    private sealed record ValidationRule(
        string FieldName,
        string JsonFieldName,
        string ErrorCode,
        string Message
    );
}
