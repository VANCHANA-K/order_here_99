using System.Net;
using System.Text.Json;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Validation;
using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Api.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static Task HandleException(HttpContext ctx, Exception ex)
    {
        var traceId =
            ctx.Response.Headers.TryGetValue("x-trace-id", out var headerTraceId)
            && !string.IsNullOrWhiteSpace(headerTraceId)
                ? headerTraceId.ToString()
                : ctx.TraceIdentifier;

        (HttpStatusCode status, string errorCode, string message) = ex switch
        {
            ConfigurationValidationException e => (
                HttpStatusCode.InternalServerError,
                e.ErrorCode,
                ResolvePublicMessage(e.ErrorCode, e.Message)
            ),
            ServiceUnavailableException e => (
                HttpStatusCode.ServiceUnavailable,
                e.ErrorCode,
                ResolvePublicMessage(e.ErrorCode, e.Message)
            ),
            InvalidRequestException e => (
                MapInvalidRequestStatus(e.ErrorCode),
                e.ErrorCode,
                ResolvePublicMessage(e.ErrorCode, e.Message)
            ),
            ConflictException e => (
                HttpStatusCode.Conflict,
                e.ErrorCode,
                ResolvePublicMessage(e.ErrorCode, e.Message)
            ),
            ImmutableResourceException e => (
                HttpStatusCode.Conflict,
                e.ErrorCode,
                ResolvePublicMessage(e.ErrorCode, e.Message)
            ),
            NotFoundException e => (
                HttpStatusCode.NotFound,
                e.ErrorCode,
                ResolvePublicMessage(e.ErrorCode, e.Message)
            ),
            DomainException e => (
                MapDomainExceptionStatus(e.ErrorCode),
                e.ErrorCode,
                ResolvePublicMessage(e.ErrorCode, e.Message)
            ),
            TimeoutException => (
                HttpStatusCode.ServiceUnavailable,
                ApplicationErrorCodes.OperationTimedOut,
                RequestValidationMessages.OperationTimedOut
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                ApiErrorCodes.UnexpectedError,
                "Unexpected error occurred."
            ),
        };

        var body = new ApiErrorResponse(errorCode, message, traceId);

        ctx.Response.ContentType = "application/json; charset=utf-8";
        ctx.Response.StatusCode = (int)status;

        return ctx.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static HttpStatusCode MapInvalidRequestStatus(string errorCode)
    {
        return errorCode switch
        {
            ApplicationErrorCodes.QrNotFound => HttpStatusCode.NotFound,
            ApplicationErrorCodes.TableNotFound => HttpStatusCode.NotFound,
            _ => HttpStatusCode.BadRequest,
        };
    }

    private static HttpStatusCode MapDomainExceptionStatus(string errorCode)
    {
        return errorCode switch
        {
            DomainErrorCodes.TableAlreadyInactive => HttpStatusCode.Conflict,
            DomainErrorCodes.TableAlreadyActive => HttpStatusCode.Conflict,
            DomainErrorCodes.TableInactive => HttpStatusCode.Conflict,
            DomainErrorCodes.OrderNotOpen => HttpStatusCode.Conflict,
            DomainErrorCodes.OrderAlreadyCompleted => HttpStatusCode.Conflict,
            DomainErrorCodes.OrderAlreadyCancelled => HttpStatusCode.Conflict,
            DomainErrorCodes.OrderAlreadyConfirmed => HttpStatusCode.Conflict,
            DomainErrorCodes.OrderItemsLocked => HttpStatusCode.Conflict,
            DomainErrorCodes.OrderCannotBeCancelled => HttpStatusCode.Conflict,
            DomainErrorCodes.OrderCannotBeConfirmed => HttpStatusCode.Conflict,
            DomainErrorCodes.CurrencyMismatch => HttpStatusCode.Conflict,
            _ => HttpStatusCode.BadRequest,
        };
    }

    private static string ResolvePublicMessage(string errorCode, string fallbackMessage)
    {
        return errorCode switch
        {
            ApplicationErrorCodes.InvalidRequest => RequestValidationMessages.InvalidRequest,
            ApplicationErrorCodes.ConfigurationInvalid => RequestValidationMessages.ConfigurationInvalid,
            ApplicationErrorCodes.AuditLogImmutable => RequestValidationMessages.AuditLogImmutable,
            ApplicationErrorCodes.OperationTimedOut => RequestValidationMessages.OperationTimedOut,
            ApplicationErrorCodes.DatabaseUnavailable =>
                RequestValidationMessages.DatabaseUnavailable,
            ApplicationErrorCodes.IdempotencyKeyRequired =>
                RequestValidationMessages.IdempotencyKeyRequired,
            ApplicationErrorCodes.IdempotencyKeyPayloadMismatch =>
                RequestValidationMessages.IdempotencyKeyPayloadMismatch,
            ApplicationErrorCodes.ConcurrencyConflict =>
                RequestValidationMessages.ConcurrencyConflict,
            ApplicationErrorCodes.RequestBodyRequired =>
                RequestValidationMessages.RequestBodyRequired,
            ApplicationErrorCodes.InvalidJson => RequestValidationMessages.InvalidJson,
            ApplicationErrorCodes.TableIdRequired => RequestValidationMessages.TableIdRequired,
            ApplicationErrorCodes.TableIdInvalid => RequestValidationMessages.TableIdInvalid,
            ApplicationErrorCodes.MenuItemIdInvalid => RequestValidationMessages.MenuItemIdInvalid,
            ApplicationErrorCodes.MenuItemIdRequired => RequestValidationMessages.MenuItemIdRequired,
            ApplicationErrorCodes.ProductNameRequired => RequestValidationMessages.ProductNameRequired,
            ApplicationErrorCodes.InvalidQuantity =>
                RequestValidationMessages.QuantityMustBeGreaterThanZero,
            ApplicationErrorCodes.EmptyItems => RequestValidationMessages.EmptyItems,
            _ => fallbackMessage
        };
    }
}
