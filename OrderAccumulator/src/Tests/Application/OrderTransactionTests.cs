using Application.Models;
using Application.Persistence;
using Application.Services;
using Application.Transactions;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Tests.Application
{
    public class OrderTransactionTests
    {
        private readonly FakeUnitOfWork _unitOfWork = new();
        private readonly FakeExposures _exposures = new();

        private OrderTransaction CreateSut() =>
            new(new OrderService(_exposures, new FakeOrders()), _unitOfWork);

        [Fact]
        public async Task Success_CommitsAndDisposes()
        {
            var result = await CreateSut().ProcessAsync(new("PETR4", OrderSide.Buy, 1, 1m), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal(["Begin", "Commit", "Dispose"], _unitOfWork.Calls);
        }

        [Fact]
        public async Task ExceptionInService_DisposesWithoutCommit_AndRethrows()
        {
            _exposures.Throw = true;

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateSut().ProcessAsync(new("PETR4", OrderSide.Buy, 1, 1m), CancellationToken.None));

            Assert.Equal(["Begin", "Dispose"], _unitOfWork.Calls);
        }

        [Fact]
        public async Task ExceptionOnCommit_Disposes_AndRethrows()
        {
            _unitOfWork.ThrowOnCommit = true;

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateSut().ProcessAsync(new("PETR4", OrderSide.Buy, 1, 1m), CancellationToken.None));

            Assert.Equal(["Begin", "Commit", "Dispose"], _unitOfWork.Calls);
        }

        private sealed class FakeUnitOfWork : IUnitOfWork, ITransaction
        {
            public List<string> Calls { get; } = [];
            public bool ThrowOnCommit { get; set; }

            public Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
            {
                Calls.Add("Begin");
                return Task.FromResult<ITransaction>(this);
            }

            public Task CommitAsync(CancellationToken ct = default)
            {
                Calls.Add("Commit");
                return ThrowOnCommit ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
            }

            public ValueTask DisposeAsync()
            {
                Calls.Add("Dispose");
                return ValueTask.CompletedTask;
            }
        }

        private sealed class FakeExposures : IAssetExposureRepository
        {
            public bool Throw { get; set; }

            public Task<AssetExposure?> GetByAssetAsync(string asset, CancellationToken ct) =>
                Throw
                    ? Task.FromException<AssetExposure?>(new InvalidOperationException())
                    : Task.FromResult<AssetExposure?>(new AssetExposure(asset));
        }

        private sealed class FakeOrders : IOrderRepository
        {
            public void Add(Order order) { }
        }
    }
}
