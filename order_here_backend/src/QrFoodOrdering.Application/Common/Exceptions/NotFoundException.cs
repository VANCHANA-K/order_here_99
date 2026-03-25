namespace QrFoodOrdering.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public string ErrorCode { get; }

    public NotFoundException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
