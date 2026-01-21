using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Domain.Orders;

public sealed class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency = "THB")
    {
        if (amount < 0)
            throw new DomainException("Money amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required");

        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(string currency = "THB") => new(0, currency);

    public Money Add(Money other)
    {
        if (other is null)
            throw new DomainException("Money is required");

        if (Currency != other.Currency)
            throw new DomainException("Currency mismatch");

        return new Money(Amount + other.Amount, Currency);
    }
}
