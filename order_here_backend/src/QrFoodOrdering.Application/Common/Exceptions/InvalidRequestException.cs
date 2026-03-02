namespace QrFoodOrdering.Application.Common.Exceptions;

public sealed class InvalidRequestException : Exception
{
    public string ErrorCode { get; }

    public InvalidRequestException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public InvalidRequestException(string message)
        : base(message)
    {
        ErrorCode = "INVALID_REQUEST";
    }
}
