using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Audit;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Tables;

namespace QrFoodOrdering.Application.Qr.Resolve;

public sealed class ResolveQrHandler
{
    private readonly IQrRepository _qrRepository;
    private readonly ITablesRepository _tablesRepository;
    private readonly IAuditService _auditService;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveQrHandler(
        IQrRepository qrRepository,
        ITablesRepository tablesRepository,
        IAuditService auditService,
        IUnitOfWork unitOfWork
    )
    {
        _qrRepository = qrRepository;
        _tablesRepository = tablesRepository;
        _auditService = auditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResolveQrResult> HandleAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidRequestException(
                ApplicationErrorCodes.QrInvalid,
                "QR token is required."
            );

        var qr = await _qrRepository.GetByTokenAsync(token, ct);

        if (qr is null)
            throw new InvalidRequestException(
                ApplicationErrorCodes.QrNotFound,
                "QR code was not found."
            );

        if (!qr.IsActive)
            throw new ConflictException(
                ApplicationErrorCodes.QrInactive,
                "This QR code is inactive."
            );

        if (qr.IsExpired())
            throw new ConflictException(
                ApplicationErrorCodes.QrExpired,
                "This QR code has expired."
            );

        var table = await _tablesRepository.GetByIdAsync(qr.TableId, ct);

        if (table is null)
            throw new InvalidRequestException(
                ApplicationErrorCodes.TableNotFound,
                "Table was not found."
            );

        if (!table.IsActive)
            throw new ConflictException(
                ApplicationErrorCodes.TableInactive,
                "This table is currently unavailable."
            );

        await _auditService.LogAsync(AuditEvents.QrResolved, AuditEntities.QrCode, qr.Id, qr.Token);
        await _unitOfWork.SaveChangesAsync(ct);

        return new ResolveQrResult { TableId = table.Id, TableCode = table.Code };
    }
}

public sealed class ResolveQrResult
{
    public Guid TableId { get; init; }
    public string TableCode { get; init; } = default!;
}
