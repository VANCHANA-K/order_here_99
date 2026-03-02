using QrFoodOrdering.Domain.Qr;

namespace QrFoodOrdering.Application.Abstractions;

public interface IQrRepository
{
    Task<QrCode?> GetByTokenAsync(string token, CancellationToken ct = default);
}
