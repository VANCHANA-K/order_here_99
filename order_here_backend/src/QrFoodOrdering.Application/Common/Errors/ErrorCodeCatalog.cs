using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Application.Common.Errors;

public static class ErrorCodeCatalog
{
    public static readonly IReadOnlyList<string> All = BuildUniqueCatalog();

    private static IReadOnlyList<string> BuildUniqueCatalog()
    {
        string[] codes =
        [
            // Application-level
            ApplicationErrorCodes.InvalidRequest,
            ApplicationErrorCodes.ConfigurationInvalid,
            ApplicationErrorCodes.AuditLogImmutable,
            ApplicationErrorCodes.OperationTimedOut,
            ApplicationErrorCodes.DatabaseUnavailable,
            ApplicationErrorCodes.IdempotencyKeyRequired,
            ApplicationErrorCodes.IdempotencyKeyConflict,
            ApplicationErrorCodes.IdempotencyKeyPayloadMismatch,
            ApplicationErrorCodes.ConcurrencyConflict,
            ApplicationErrorCodes.RequestBodyRequired,
            ApplicationErrorCodes.InvalidJson,
            ApplicationErrorCodes.TableIdRequired,
            ApplicationErrorCodes.TableIdInvalid,
            ApplicationErrorCodes.MenuItemIdInvalid,
            ApplicationErrorCodes.MenuItemIdRequired,
            ApplicationErrorCodes.OrderNotFound,
            ApplicationErrorCodes.ProductNameRequired,
            ApplicationErrorCodes.UnitPriceInvalid,
            ApplicationErrorCodes.TableNotFound,
            ApplicationErrorCodes.TableInactive,
            ApplicationErrorCodes.TableCodeAlreadyExists,
            ApplicationErrorCodes.QrInvalid,
            ApplicationErrorCodes.QrNotFound,
            ApplicationErrorCodes.QrInactive,
            ApplicationErrorCodes.QrExpired,
            ApplicationErrorCodes.QrTokenAlreadyExists,
            ApplicationErrorCodes.EmptyItems,
            ApplicationErrorCodes.InvalidQuantity,
            ApplicationErrorCodes.ItemNotFound,
            ApplicationErrorCodes.ItemInactive,
            ApplicationErrorCodes.ItemUnavailable,
            ApplicationErrorCodes.MenuCodeAlreadyExists,
            // Domain-level
            DomainErrorCodes.OrderIdRequired,
            DomainErrorCodes.TableIdRequired,
            DomainErrorCodes.ItemRequired,
            DomainErrorCodes.OrderAlreadyCompleted,
            DomainErrorCodes.OrderAlreadyCancelled,
            DomainErrorCodes.OrderAlreadyConfirmed,
            DomainErrorCodes.OrderNotOpen,
            DomainErrorCodes.OrderItemsLocked,
            DomainErrorCodes.OrderCannotBeCancelled,
            DomainErrorCodes.OrderCannotBeConfirmed,
            DomainErrorCodes.OrderItemIdRequired,
            DomainErrorCodes.ProductNameRequired,
            DomainErrorCodes.QuantityInvalid,
            DomainErrorCodes.UnitPriceRequired,
            DomainErrorCodes.MoneyAmountNegative,
            DomainErrorCodes.CurrencyRequired,
            DomainErrorCodes.MoneyRequired,
            DomainErrorCodes.CurrencyMismatch,
            DomainErrorCodes.TableCodeRequired,
            DomainErrorCodes.TableAlreadyInactive,
            DomainErrorCodes.TableAlreadyActive,
            DomainErrorCodes.TableInactive,
        ];

        return codes.Distinct(StringComparer.Ordinal).ToArray();
    }
}
