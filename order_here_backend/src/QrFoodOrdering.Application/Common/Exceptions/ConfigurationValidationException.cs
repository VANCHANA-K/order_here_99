namespace QrFoodOrdering.Application.Common.Exceptions;

public sealed class ConfigurationValidationException : Exception
{
    public string ErrorCode { get; }

    public ConfigurationValidationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
