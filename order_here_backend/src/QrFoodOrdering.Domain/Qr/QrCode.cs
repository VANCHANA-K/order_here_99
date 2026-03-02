namespace QrFoodOrdering.Domain.Qr;

public class QrCode
{
    public Guid Id { get; private set; }
    public Guid TableId { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private QrCode() { }

    public QrCode(Guid tableId, string token, DateTime expiresAtUtc)
    {
        Id = Guid.NewGuid();
        TableId = tableId;
        Token = token;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
        IsActive = true;
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAtUtc;

    public void Deactivate() => IsActive = false;
}
