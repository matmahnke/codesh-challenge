using Api.Contracts;
using Api.Validators;

namespace Tests.Api
{
    public class CreateOrderRequestValidatorTests
    {
        private readonly CreateOrderRequestValidator _validator = new();

        private static CreateOrderRequest Valid() => new()
        {
            Ativo = "PETR4",
            Lado = "C",
            Quantidade = 584,
            Preco = 54.87m
        };

        [Fact]
        public void ValidRequest_Passes()
        {
            Assert.True(_validator.Validate(Valid()).IsValid);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("XPTO3")]
        public void InvalidAsset_Fails(string? ativo) =>
            AssertSingleError(new() { Ativo = ativo, Lado = "C", Quantidade = 1, Preco = 1m }, nameof(CreateOrderRequest.Ativo));

        [Theory]
        [InlineData(null)]
        [InlineData("X")]
        [InlineData("c")]
        public void InvalidSide_Fails(string? lado) =>
            AssertSingleError(new() { Ativo = "PETR4", Lado = lado, Quantidade = 1, Preco = 1m }, nameof(CreateOrderRequest.Lado));

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(100_000)]
        public void InvalidQuantity_Fails(int? quantidade) =>
            AssertSingleError(new() { Ativo = "PETR4", Lado = "C", Quantidade = quantidade, Preco = 1m }, nameof(CreateOrderRequest.Quantidade));

        [Theory]
        [InlineData(null)]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("1000")]
        [InlineData("10.001")]
        public void InvalidPrice_Fails(string? preco) =>
            AssertSingleError(
                new() { Ativo = "PETR4", Lado = "C", Quantidade = 1, Preco = preco is null ? null : decimal.Parse(preco, System.Globalization.CultureInfo.InvariantCulture) },
                nameof(CreateOrderRequest.Preco));

        [Fact]
        public void AllErrorsAreReportedTogether()
        {
            var result = _validator.Validate(new CreateOrderRequest { Ativo = "XPTO3", Lado = "X", Quantidade = 100_000, Preco = 10.001m });

            Assert.Equal(4, result.Errors.Count);
        }

        private void AssertSingleError(CreateOrderRequest request, string property)
        {
            var result = _validator.Validate(request);

            var error = Assert.Single(result.Errors);
            Assert.Equal(property, error.PropertyName);
        }
    }
}
