namespace QrFoodOrdering.Api.Contracts.Qr;

public sealed record GenerateQrResponse(
    Guid TableId,
    string Token,
    string QrUrl,
    DateTime GeneratedAtUtc
);
