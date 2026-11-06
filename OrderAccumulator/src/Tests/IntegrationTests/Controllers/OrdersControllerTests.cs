using Api.Contracts;
using Domain;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Tests.IntegrationTests.Fixtures;

namespace Tests.IntegrationTests.Controllers
{
    public class OrdersControllerTests : IAsyncLifetime
    {
        private const string Route = "/api/orders";

        private readonly OrderApiFactory _factory = new();
        private HttpClient _client = null!;

        public Task InitializeAsync()
        {
            _client = _factory.CreateClient();
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _client.Dispose();
            await _factory.DisposeAsync();
        }

        private static object Request(string ativo = Assets.PETR4, string lado = "C", int quantidade = 584, decimal preco = 54.87m) =>
            new { ativo, lado, quantidade, preco };

        private async Task<(HttpStatusCode Status, CreateOrderResponse Body)> PostAsync(object request)
        {
            var response = await _client.PostAsJsonAsync(Route, request);
            var body = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
            return (response.StatusCode, body!);
        }

        private Task<AssetExposure> ExposureOf(string asset) =>
            _factory.WithDbContextAsync(db => db.AssetExposures.AsNoTracking().SingleAsync(x => x.Asset == asset));

        private Task<List<Order>> PersistedOrders() =>
            _factory.WithDbContextAsync(db => db.Orders.AsNoTracking().ToListAsync());

        [Fact]
        public async Task Post_ValidBuy_ReturnsOk_AndPersistsOrderAndExposure()
        {
            // Arrange
            var request =  Request();

            // Act
            var (status, body) = await PostAsync(request);

            // Assert
            status.Should().Be(HttpStatusCode.OK);
            body.Sucesso.Should().BeTrue();
            body.ExposicaoAtual.Should().Be(32044.08m);
            body.MsgErro.Should().BeNull();

            var exposure = await ExposureOf(Assets.PETR4);
            exposure.ExposureCents.Should().Be(3_204_408);
            exposure.Version.Should().Be(1);

            var order = (await PersistedOrders()).Should().ContainSingle().Subject;
            order.Asset.Should().Be(Assets.PETR4);
            order.Side.Should().Be(OrderSide.Buy);
            order.Quantity.Should().Be(584);
            order.PriceCents.Should().Be(5487);
            order.Status.Should().Be(OrderStatus.Accepted);
        }

        [Fact]
        public async Task Post_ValidSell_ReturnsNegativeExposure()
        {
            // Arrange
            var request = Request(Assets.VALE3, "V", 10, 100m);

            var (status, body) = await PostAsync(request);

            // Assert
            status.Should().Be(HttpStatusCode.OK);
            body.Sucesso.Should().BeTrue();
            body.ExposicaoAtual.Should().Be(-1000m);
        }

        [Fact]
        public async Task Post_SequentialOrders_AcumulateExposurePerAsset()
        {
            // Arrange
            await PostAsync(Request(Assets.PETR4, "C", 10, 100m));
            await PostAsync(Request(Assets.VALE3, "C", 1, 1m));

            // Act
            var (_, body) = await PostAsync(Request(Assets.PETR4, "V", 4, 100m));

            // Assert
            body.ExposicaoAtual.Should().Be(600m);
            (await ExposureOf(Assets.PETR4)).Version.Should().Be(2);
            (await ExposureOf(Assets.VALE3)).ExposureCents.Should().Be(100);
            (await PersistedOrders()).Should().HaveCount(3);
        }

        [Fact]
        public async Task Post_ExceedingLimit_ReturnsOkWithError_KeepsExposure_AndPersistsRejectedOrder()
        {
            // Arrange
            await PostAsync(Request(Assets.PETR4, "C", 1000, 999.99m));
            var before = await ExposureOf(Assets.PETR4);

            // Act
            var (status, body) = await PostAsync(Request(Assets.PETR4, "C", 99_999, 999.99m));

            // Assert
            status.Should().Be(HttpStatusCode.OK);
            body.Sucesso.Should().BeFalse();
            body.ExposicaoAtual.Should().Be(before.Exposure);
            body.MsgErro.Should().StartWith("Ordem rejeitada:").And.Contain(Assets.PETR4);

            (await ExposureOf(Assets.PETR4)).ExposureCents.Should().Be(before.ExposureCents);
            var orders = await PersistedOrders();
            orders.Should().HaveCount(2);
            orders.Should().ContainSingle(o => o.Status == OrderStatus.Rejected)
                .Which.RejectionReason.Should().Be(body.MsgErro);
        }

        [Fact]
        public async Task Post_InvalidAsset_ReturnsBadRequest_AndPersistsNothnig()
        {
            // Arrange
            var request = Request(ativo: "XPTO3");

            // Act
            var (status, body) = await PostAsync(request);

            // Assert
            status.Should().Be(HttpStatusCode.BadRequest);
            body.Sucesso.Should().BeFalse();
            body.ExposicaoAtual.Should().Be(0m);
            body.MsgErro.Should().Be("Ativo deve ser PETR4, VALE3, VIIA4.");
            (await PersistedOrders()).Should().BeEmpty();
        }

        [Fact]
        public async Task Post_EmptyBody_ReturnsAllValidationErrorsJoined()
        {
            // Arrange
            var request = new { };

            // Act
            var (status, body) = await PostAsync(request);

            // Assert
            status.Should().Be(HttpStatusCode.BadRequest);
            body.MsgErro.Should().Be(
                "Ativo é obrigatório.; Lado é obrigatório.; Quantidade é obrigatória.; Preço é obrigatório.");
        }

        [Theory]
        [InlineData("""{"ativo":"PETR4","lado":"C","quantidade":"abc","preco":10}""")]
        [InlineData("""{"ativo":"PETR4",""")]
        public async Task Post_MalformedJson_ReturnsBadRequestWithGenericMessage(string json)
        {
            // Arrange
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync(Route, content);
            var body = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            body!.Sucesso.Should().BeFalse();
            body.MsgErro.Should().Be("JSON da requisição inválido ou com tipos incorretos.");
        }

        [Fact]
        public async Task Post_ConcurrentOrders_KeepExposureConsistentWithAcceptedOrders()
        {
            // Arrange
            var requests = Enumerable.Range(0, 10).Select(_ => Request(Assets.VIIA4, "C", 1, 1m));

            // Act
            var responses = await Task.WhenAll(requests.Select(PostAsync));

            // Assert
            responses.Select(r => r.Status).Should()
                .OnlyContain(s => s == HttpStatusCode.OK || s == HttpStatusCode.Conflict);


            var acceptedCount = responses.Count(r => r.Status == HttpStatusCode.OK && r.Body.Sucesso);
            var exposure = await ExposureOf(Assets.VIIA4);
            exposure.ExposureCents.Should().Be(acceptedCount * 100);
            exposure.Version.Should().Be(acceptedCount);
            (await PersistedOrders()).Should().HaveCount(acceptedCount);
        }
    }
}
