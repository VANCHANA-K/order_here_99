using System;
using System.Threading;
using System.Threading.Tasks;

namespace QrFoodOrdering.Application.Abstractions;

public interface IUnitOfWork
{
    Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);
}

