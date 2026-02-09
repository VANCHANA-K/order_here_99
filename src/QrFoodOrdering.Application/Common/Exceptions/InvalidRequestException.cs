namespace QrFoodOrdering.Application.Common.Exceptions;

public sealed class InvalidRequestException : Exception
{
    public InvalidRequestException(string message)
        : base(message) { }
}
