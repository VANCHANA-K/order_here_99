// File: src/QrFoodOrdering.Application/Common/Idempotency/IdempotencyResult.cs
using System;

namespace QrFoodOrdering.Application.Common.Idempotency;

public readonly record struct IdempotencyResult(bool Found, Guid OrderId);
