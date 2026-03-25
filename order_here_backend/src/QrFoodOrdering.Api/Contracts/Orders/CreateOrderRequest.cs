using System.ComponentModel.DataAnnotations;
using QrFoodOrdering.Api.Validation;
using QrFoodOrdering.Application.Common.Validation;

namespace QrFoodOrdering.Api.Contracts.Orders;

public sealed record CreateOrderRequest(
    [param: NotEmptyGuid(ErrorMessage = RequestValidationMessages.TableIdRequired)]
    Guid TableId
);
