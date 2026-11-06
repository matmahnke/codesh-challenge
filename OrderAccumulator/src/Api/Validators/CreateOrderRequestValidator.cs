using System.Globalization;
using Api.Contracts;
using Domain;
using Domain.Entities;
using FluentValidation;

namespace Api.Validators
{
    public sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        private static readonly string[] AllowedSides = ["C", "V"];
        private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");
        private const decimal MaxPriceExclusive = Order.MaxPriceCentsExclusive / 100m;

        public CreateOrderRequestValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Ativo)
                .NotEmpty().WithMessage("Ativo é obrigatório.")
                .Must(ativo => Assets.IsSupported(ativo!))
                    .WithMessage($"Ativo deve ser {string.Join(", ", Assets.Supported)}.");

            RuleFor(x => x.Lado)
                .NotEmpty().WithMessage("Lado é obrigatório.")
                .Must(lado => AllowedSides.Contains(lado))
                    .WithMessage("Lado deve ser 'C' (compra) ou 'V' (venda).");

            RuleFor(x => x.Quantidade)
                .NotNull().WithMessage("Quantidade é obrigatória.")
                .GreaterThan(0).WithMessage("Quantidade deve ser um inteiro positivo.")
                .LessThan(Order.MaxQuantityExclusive)
                    .WithMessage($"Quantidade deve ser menor que {Order.MaxQuantityExclusive.ToString("N0", PtBr)}.");

            RuleFor(x => x.Preco)
                .NotNull().WithMessage("Preço é obrigatório.")
                .GreaterThan(0).WithMessage("Preço deve ser positivo.")
                .LessThan(MaxPriceExclusive)
                    .WithMessage($"Preço deve ser menor que {MaxPriceExclusive.ToString("N0", PtBr)}.")
                .Must(BeMultipleOfOneCent).WithMessage("Preço deve ser múltiplo de 0.01.");
        }

        private static bool BeMultipleOfOneCent(decimal? preco) =>
            preco is null || preco * 100 == decimal.Truncate(preco.Value * 100);
    }
}
