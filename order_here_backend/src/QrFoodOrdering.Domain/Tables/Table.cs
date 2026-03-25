using QrFoodOrdering.Domain.Common;

namespace QrFoodOrdering.Domain.Tables;

public sealed class Table
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = default!;

    public bool IsActive { get; private set; }

    // For EF Core
    private Table() { }

    public Table(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException(DomainErrorCodes.TableCodeRequired, "Table code is required");

        Id = Guid.NewGuid();
        Code = code;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(
                DomainErrorCodes.TableAlreadyInactive,
                "Table is already inactive"
            );

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException(
                DomainErrorCodes.TableAlreadyActive,
                "Table is already active"
            );

        IsActive = true;
    }

    // ✅ BE-33 reusable validation
    public void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException(DomainErrorCodes.TableInactive, "Table is inactive");
    }
}
