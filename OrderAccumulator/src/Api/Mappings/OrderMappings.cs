using Api.Contracts;
using Application.Models;
using Domain.Enums;
using FluentValidation.Results;
using System.Diagnostics;

namespace Api.Mappings
{
    public static class OrderMappings
    {
        public static CreateOrderCommand ToCommand(this CreateOrderRequest request) =>
            new(request.Ativo!, ToSide(request.Lado), request.Quantidade!.Value, request.Preco!.Value);

        public static CreateOrderResponse ToResponse(this CreateOrderResult result) =>
            new(result.Success, result.CurrentExposure, result.ErrorMessage);

        public static CreateOrderResponse ToResponse(this ValidationResult validation) =>
            CreateOrderResponse.Failure(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));

        private static OrderSide ToSide(string? lado) => lado switch
        {
            "C" => OrderSide.Buy,
            "V" => OrderSide.Sell,
            _ => throw new UnreachableException($"Lado '{lado}' deveria ter sido rejeitado na validação.")
        };
    }
}
