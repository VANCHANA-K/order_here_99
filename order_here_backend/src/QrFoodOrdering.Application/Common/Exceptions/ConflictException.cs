namespace QrFoodOrdering.Application.Common.Exceptions;

public sealed class ConflictException : Exception
{
    public string ErrorCode { get; }

    public ConflictException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
