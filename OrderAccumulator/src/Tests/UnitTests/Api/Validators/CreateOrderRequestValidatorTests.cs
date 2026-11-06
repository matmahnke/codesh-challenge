using Api.Contracts;
using Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;
using System.Globalization;

namespace Tests.UnitTests.Api.Validators
{
    public class CreateOrderRequestValidatorTests
    {
        private readonly CreateOrderRequestValidator _validator = new();

        private static decimal D(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);

        private static CreateOrderRequest Valid(
            string? ativo = "PETR4",
            string? lado = "C",
            int? quantidade = 584,
            decimal? preco = 54.87m) => new()
            {
                Ativo = ativo,
                Lado = lado,
                Quantidade = quantidade,
                Preco = preco
            };

        [Fact]
        public void ValidRequest_HasNoErrors()
        {
            // Arrange
            var request = Valid();

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.IsValid.Should().BeTrue();
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("PETR4")]
        [InlineData("VALE3")]
        [InlineData("VIIA4")]
        public void SupportedAsset_IsValid(string ativo)
        {
            // Arrange
            var request = Valid(ativo: ativo);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Ativo);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingAsset_Fails(string? ativo)
        {
            // Arrange
            var request = Valid(ativo: ativo);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Ativo)
                .WithErrorMessage("Ativo é obrigatório.")
                .Only();
        }

        [Theory]
        [InlineData("XPTO3")]
        [InlineData("petr4")]
        public void UnsupportedAsset_Fails(string ativo)
        {
            // Arrange
            var request = Valid(ativo: ativo);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Ativo)
                .WithErrorMessage("Ativo deve ser PETR4, VALE3, VIIA4.")
                .Only();
        }

        [Theory]
        [InlineData("C")]
        [InlineData("V")]
        public void AllowedSide_IsValid(string lado)
        {
            // Arrange
            var request = Valid(lado: lado);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Lado);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MissingSide_Fails(string? lado)
        {
            // Arrange
            var request = Valid(lado: lado);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Lado)
                .WithErrorMessage("Lado é obrigatório.")
                .Only();
        }

        [Theory]
        [InlineData("c")]
        [InlineData("X")]
        [InlineData("CV")]
        public void InvalidSide_Fails(string lado)
        {
            // Arrange
            var request = Valid(lado: lado);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Lado)
                .WithErrorMessage("Lado deve ser 'C' (compra) ou 'V' (venda).")
                .Only();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(99_999)]
        public void QuantityWithinRange_IsValid(int quantidade)
        {
            // Arrange
            var request = Valid(quantidade: quantidade);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Quantidade);
        }

        [Fact]
        public void MissingQuantity_Fails()
        {
            // Arrange
            var request = Valid(quantidade: null);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantidade)
                .WithErrorMessage("Quantidade é obrigatória.")
                .Only();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void NonPositiveQuantity_Fails(int quantidade)
        {
            // Arrange
            var request = Valid(quantidade: quantidade);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantidade)
                .WithErrorMessage("Quantidade deve ser um inteiro positivo.")
                .Only();
        }

        [Theory]
        [InlineData(100_000)]
        [InlineData(int.MaxValue)]
        public void QuantityAtOrAboveLimit_Fails(int quantidade)
        {
            // Arrange
            var request = Valid(quantidade: quantidade);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantidade)
                .WithErrorMessage("Quantidade deve ser menor que 100.000.")
                .Only();
        }

        [Theory]
        [InlineData("0.01")]
        [InlineData("999.99")]
        [InlineData("10.5")]
        public void PriceWithinRange_IsValid(string preco)
        {
            // Arrange
            var request = Valid(preco: D(preco));

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Preco);
        }

        [Fact]
        public void MissingPrice_Fails()
        {
            // Arrange
            var request = Valid(preco: null);

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Preco)
                .WithErrorMessage("Preço é obrigatório.")
                .Only();
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-0.01")]
        public void NonPositivePrice_Fails(string preco)
        {
            // Arrange
            var request = Valid(preco: D(preco));

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Preco)
                .WithErrorMessage("Preço deve ser positivo.")
                .Only();
        }

        [Theory]
        [InlineData("1000")]
        [InlineData("1000.01")]
        public void PriceAtOrAboveLimit_Fails(string preco)
        {
            // Arrange
            var request = Valid(preco: D(preco));

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Preco)
                .WithErrorMessage("Preço deve ser menor que 1.000.")
                .Only();
        }

        [Theory]
        [InlineData("10.001")]
        [InlineData("0.005")]
        public void PriceNotMultipleOfOneCent_Fails(string preco)
        {
            // Arrange
            var request = Valid(preco: D(preco));

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Preco)
                .WithErrorMessage("Preço deve ser múltiplo de 0.01.")
                .Only();
        }

        [Fact]
        public void EmptyRequest_ReportsOneErrorPerField_InDeclarationOrder()
        {
            // Arrange
            var request = new CreateOrderRequest();

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.Errors.Select(e => e.ErrorMessage).Should().Equal(
                "Ativo é obrigatório.",
                "Lado é obrigatório.",
                "Quantidade é obrigatória.",
                "Preço é obrigatório.");
        }
    }
}
