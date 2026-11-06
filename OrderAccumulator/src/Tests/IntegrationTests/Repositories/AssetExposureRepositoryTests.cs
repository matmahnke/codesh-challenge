using Domain;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Tests.IntegrationTests.Fixtures;

namespace Tests.IntegrationTests.Repositories
{
    public class AssetExposureRepositoryTests : IAsyncLifetime
    {
        private readonly SqliteDatabase _db = new();

        public Task InitializeAsync() => _db.InitializeAsync();

        public Task DisposeAsync() => _db.DisposeAsync();

        [Theory]
        [InlineData(Assets.PETR4)]
        [InlineData(Assets.VALE3)]
        [InlineData(Assets.VIIA4)]
        public async Task GetByAssetAsync_SeededAsset_ReturnsZeroExposure(string asset)
        {
            // Arrange
            await using var context = _db.CreateContext();
            var sut = new AssetExposureRepository(context);

            // Act
            var exposure = await sut.GetByAssetAsync(asset, CancellationToken.None);

            // Assert
            exposure.Should().NotBeNull();
            exposure!.Asset.Should().Be(asset);
            exposure.ExposureCents.Should().Be(0);
            exposure.Version.Should().Be(0);
        }

        [Fact]
        public async Task GetByAssetAsync_UnknownAsset_ReturnsNull()
        {
            // Arrange
            await using var context = _db.CreateContext();
            var sut = new AssetExposureRepository(context);

            // Akt
            var exposure = await sut.GetByAssetAsync("XPTO3", CancellationToken.None);

            // Assert
            exposure.Should().BeNull();
        }

        [Fact]
        public async Task GetByAssetAsync_ReturnsTrackedEntity_SoChangesArePersisted()
        {
            // Arrange
            await using (var context = _db.CreateContext())
            {
                var sut = new AssetExposureRepository(context);
                var exposure = await sut.GetByAssetAsync(Assets.PETR4, CancellationToken.None);
                exposure!.TryApply(Order.Create(Assets.PETR4, OrderSide.Buy, 584, 54.87m));

                // Act
                await context.SaveChangesAsync();
            }

            //  Assert
            await using var readContext = _db.CreateContext();
            var persisted = await new AssetExposureRepository(readContext)
                .GetByAssetAsync(Assets.PETR4, CancellationToken.None);
            persisted!.ExposureCents.Should().Be(3_204_408);
            persisted.Version.Should().Be(1);
        }

        [Fact]
        public async Task StaleVersion_OnSave_ThrowsConcurrencyException()
        {
            // Arrange
            await using var first = _db.CreateContext();
            await using var second = _db.CreateContext();
            var exposureA = await new AssetExposureRepository(first).GetByAssetAsync(Assets.VALE3, CancellationToken.None);
            var exposureB = await new AssetExposureRepository(second).GetByAssetAsync(Assets.VALE3, CancellationToken.None);

            exposureA!.TryApply(Order.Create(Assets.VALE3, OrderSide.Buy, 10, 10m));
            exposureB!.TryApply(Order.Create(Assets.VALE3, OrderSide.Sell, 5, 10m));
            await first.SaveChangesAsync();

            // Act
            var act = () => second.SaveChangesAsync();

            // Assert
            await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
            var cents = await _db.ScalarAsync("SELECT ExposureCents FROM AssetExposures WHERE Asset = 'VALE3'");
            cents.Should().Be(10_000L);


        }

        [Fact]
        public async Task GetByAssetAsync_CanceledToken_Throws()
        {
            // Arrange
            await using var context = _db.CreateContext();
            var sut = new AssetExposureRepository(context);
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act
            var act = () => sut.GetByAssetAsync(Assets.PETR4, cts.Token);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }
    }
}
