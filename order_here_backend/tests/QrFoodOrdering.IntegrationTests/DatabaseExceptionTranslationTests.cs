using Microsoft.Extensions.DependencyInjection;
using QrFoodOrdering.Application.Abstractions;
using QrFoodOrdering.Application.Common.Errors;
using QrFoodOrdering.Application.Common.Exceptions;
using QrFoodOrdering.Domain.Menu;
using QrFoodOrdering.Domain.Qr;
using QrFoodOrdering.Infrastructure.Persistence;
using QrFoodOrdering.IntegrationTests.Infrastructure;

namespace QrFoodOrdering.IntegrationTests;

public sealed class DatabaseExceptionTranslationTests
{
    [Fact]
    public async Task Unit_of_work_should_translate_duplicate_qr_token_to_conflict_exception()
    {
        await using var factory = new TestApiFactory();

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            factory.ExecuteScopedAsync(async services =>
            {
                var db = services.GetRequiredService<QrFoodOrderingDbContext>();
                var uow = services.GetRequiredService<IUnitOfWork>();

                db.QrCodes.Add(new QrCode(Guid.NewGuid(), "dup-token", DateTime.UtcNow.AddDays(1)));
                await uow.SaveChangesAsync(CancellationToken.None);

                db.QrCodes.Add(new QrCode(Guid.NewGuid(), "dup-token", DateTime.UtcNow.AddDays(1)));
                await uow.SaveChangesAsync(CancellationToken.None);
            })
        );

        Assert.Equal(ApplicationErrorCodes.QrTokenAlreadyExists, ex.ErrorCode);
        Assert.Equal("QR token already exists.", ex.Message);
    }

    [Fact]
    public async Task Unit_of_work_should_translate_duplicate_menu_code_to_conflict_exception()
    {
        await using var factory = new TestApiFactory();

        var ex = await Assert.ThrowsAsync<ConflictException>(() =>
            factory.ExecuteScopedAsync(async services =>
            {
                var db = services.GetRequiredService<QrFoodOrderingDbContext>();
                var uow = services.GetRequiredService<IUnitOfWork>();

                db.MenuItems.Add(new MenuItem("M-DUP", "Pad Thai", 60m));
                await uow.SaveChangesAsync(CancellationToken.None);

                db.MenuItems.Add(new MenuItem("M-DUP", "Fried Rice", 55m));
                await uow.SaveChangesAsync(CancellationToken.None);
            })
        );

        Assert.Equal(ApplicationErrorCodes.MenuCodeAlreadyExists, ex.ErrorCode);
        Assert.Equal("Menu code already exists.", ex.Message);
    }
}
