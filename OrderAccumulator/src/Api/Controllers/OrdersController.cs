using Api.Contracts;
using Api.Mappings;
using Application.Transactions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    public sealed class OrdersController(
        IOrderTransaction orderTransaction,
        IValidator<CreateOrderRequest> validator) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType<CreateOrderResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<CreateOrderResponse>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<CreateOrderResponse>(StatusCodes.Status409Conflict)]
        [ProducesResponseType<CreateOrderResponse>(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateOrderResponse>> Create(
            [FromBody] CreateOrderRequest request,
            CancellationToken ct)
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return BadRequest(validation.ToResponse());

            var result = await orderTransaction.ProcessAsync(request.ToCommand(), ct);

            return Ok(result.ToResponse());
        }
    }
}
