using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Tables;
using QrFoodOrdering.Domain.Qr;

namespace QrFoodOrdering.Application.Qr.Generate;

public sealed class GenerateQrHandler
{
    private readonly IQrRepository _qrRepository;
    private readonly ITablesRepository _tablesRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _auditService;

    public GenerateQrHandler(
        IQrRepository qrRepository,
        ITablesRepository tablesRepository,
        IUnitOfWork unitOfWork,
        IAuditService auditService
    )
    {
        _qrRepository = qrRepository;
        _tablesRepository = tablesRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
    }

    public async Task<GenerateQrResult> HandleAsync(Guid tableId, CancellationToken ct = default)
    {
        var table = await _tablesRepository.GetByIdAsync(tableId, ct);

        if (table is null)
            throw new InvalidRequestException(
                ApplicationErrorCodes.TableNotFound,
                "Table not found."
            );

        if (!table.IsActive)
            throw new ConflictException(
                ApplicationErrorCodes.TableInactive,
                "Cannot generate QR for inactive table."
            );

        // BE-37 Logic
        var activeQrs = await _qrRepository.GetActiveByTableIdAsync(tableId, ct);

        foreach (var qr in activeQrs)
        {
            qr.Deactivate();
        }

        // create new QR
        var newQr = QrCode.Create(tableId);

        await _qrRepository.AddAsync(newQr, ct);

        await _auditService.LogAsync(
            AuditEvents.QrGenerated,
            AuditEntities.Table,
            table.Id,
            newQr.Token);

        await _unitOfWork.SaveChangesAsync(ct);

        return new GenerateQrResult
        {
            TableId = table.Id,
            Token = newQr.Token,
            QrUrl = $"https://localhost:3000/order/{newQr.Token}",
            GeneratedAtUtc = newQr.CreatedAtUtc
        };
    }
}

public sealed class GenerateQrResult
{
    public Guid TableId { get; init; }
    public string Token { get; init; } = default!;
    public string QrUrl { get; init; } = default!;
    public DateTime GeneratedAtUtc { get; init; }
}
