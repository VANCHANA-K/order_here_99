using QrFoodOrdering.Domain.Qr;

namespace QrFoodOrdering.Application.Abstractions;

public interface IQrRepository
{
    Task AddAsync(QrCode qrCode, CancellationToken ct);
    Task<QrCode?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task<List<QrCode>> GetActiveByTableIdAsync(Guid tableId, CancellationToken ct = default);
}
