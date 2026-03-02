using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Tables;

namespace QrFoodOrdering.Application.Qr.Resolve;

public sealed class ResolveQrHandler
{
    private readonly IQrRepository _qrRepository;
    private readonly ITablesRepository _tablesRepository;

    public ResolveQrHandler(IQrRepository qrRepository, ITablesRepository tablesRepository)
    {
        _qrRepository = qrRepository;
        _tablesRepository = tablesRepository;
    }

    public async Task<ResolveQrResult> HandleAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidRequestException("QR token is required.");

        var qr = await _qrRepository.GetByTokenAsync(token, ct);

        if (qr is null)
            throw new InvalidRequestException("QR token not found.");

        if (!qr.IsActive)
            throw new ConflictException("QR_INACTIVE", "This QR code is inactive.");

        if (qr.IsExpired())
            throw new ConflictException("QR_EXPIRED", "QR code has expired.");

        var table = await _tablesRepository.GetByIdAsync(qr.TableId, ct);

        if (table is null)
            throw new InvalidRequestException("Table not found.");

        if (!table.IsActive)
            throw new ConflictException("TABLE_INACTIVE", "This table is currently inactive.");

        return new ResolveQrResult { TableId = table.Id, TableCode = table.Code };
    }
}

public sealed class ResolveQrResult
{
    public Guid TableId { get; init; }
    public string TableCode { get; init; } = default!;
}
