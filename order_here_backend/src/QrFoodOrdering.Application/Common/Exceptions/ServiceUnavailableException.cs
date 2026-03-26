namespace QrFoodOrdering.Application.Common.Exceptions;

public sealed class ServiceUnavailableException : Exception
{
    public string ErrorCode { get; }

    public ServiceUnavailableException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
