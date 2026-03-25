using System.Net;
using System.Text.Json;
using QrFoodOrdering.Api.Contracts.Common;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
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
            InvalidRequestException e => (
                MapInvalidRequestStatus(e.ErrorCode),
                e.ErrorCode,
                e.Message
            ),
            ConflictException e => (HttpStatusCode.Conflict, e.ErrorCode, e.Message),
            NotFoundException e => (HttpStatusCode.NotFound, e.ErrorCode, e.Message),
            DomainException e => (MapDomainExceptionStatus(e.ErrorCode), e.ErrorCode, e.Message),

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
            DomainErrorCodes.CurrencyMismatch => HttpStatusCode.Conflict,
            _ => HttpStatusCode.BadRequest,
        };
    }
}
