using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;

namespace QrFoodOrdering.Infrastructure.Persistence;

internal static class DbUpdateExceptionTranslator
{
    public static Exception Translate(DbUpdateException ex)
    {
        if (TryTranslateSqliteUniqueViolation(ex, out var translated))
            return translated;

        return ex;
    }

    private static bool TryTranslateSqliteUniqueViolation(
        DbUpdateException ex,
        out Exception translated
    )
    {
        translated = default!;

        if (ex.InnerException is not SqliteException sqliteEx || sqliteEx.SqliteErrorCode != 19)
            return false;

        translated = sqliteEx.Message switch
        {
            var message when message.Contains("tables.Code", StringComparison.Ordinal) =>
                new ConflictException(
                    ApplicationErrorCodes.TableCodeAlreadyExists,
                    "Table code already exists."
                ),
            var message when message.Contains("qr_codes.Token", StringComparison.Ordinal) =>
                new ConflictException(
                    ApplicationErrorCodes.QrTokenAlreadyExists,
                    "QR token already exists."
                ),
            var message when message.Contains("MenuItems.Code", StringComparison.Ordinal) =>
                new ConflictException(
                    ApplicationErrorCodes.MenuCodeAlreadyExists,
                    "Menu code already exists."
                ),
            _ => ex
        };

        return translated is not DbUpdateException;
    }
}
