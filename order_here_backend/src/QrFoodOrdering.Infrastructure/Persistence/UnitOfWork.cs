using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.Sqlite;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Application.Common.Validation;
using QrFoodOrdering.Infrastructure.Persistence;

namespace QrFoodOrdering.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly QrFoodOrderingDbContext _db;
    private IDbContextTransaction? _tx;

    public UnitOfWork(QrFoodOrderingDbContext db)
    {
        _db = db;
    }

    public async Task<IAsyncDisposable> BeginTransactionAsync(CancellationToken ct)
    {
        _tx = await _db.Database.BeginTransactionAsync(ct);
        return _tx;
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_tx is not null)
        {
            await _tx.CommitAsync(ct);
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                ApplicationErrorCodes.ConcurrencyConflict,
                RequestValidationMessages.ConcurrencyConflict
            );
        }
        catch (DbUpdateException ex)
        {
            throw DbUpdateExceptionTranslator.Translate(ex);
        }
        catch (TimeoutException)
        {
            throw new ServiceUnavailableException(
                ApplicationErrorCodes.OperationTimedOut,
                RequestValidationMessages.OperationTimedOut
            );
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ServiceUnavailableException(
                ApplicationErrorCodes.OperationTimedOut,
                RequestValidationMessages.OperationTimedOut
            );
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode is 5 or 14)
        {
            throw new ServiceUnavailableException(
                ApplicationErrorCodes.DatabaseUnavailable,
                RequestValidationMessages.DatabaseUnavailable
            );
        }
    }
}
