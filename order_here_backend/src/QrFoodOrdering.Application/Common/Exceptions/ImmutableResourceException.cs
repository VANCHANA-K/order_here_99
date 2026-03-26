namespace QrFoodOrdering.Application.Common.Exceptions;

public sealed class ImmutableResourceException : Exception
{
    public string ErrorCode { get; }

    public ImmutableResourceException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
