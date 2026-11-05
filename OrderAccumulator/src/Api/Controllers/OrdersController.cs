using Api.Contracts;
using Application.Models;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController(IOrderService orderService) : ControllerBase
    {

        [HttpPost]
        public async Task<ActionResult<CreateOrderResponse>> Create(
            [FromBody] CreateOrderRequest request, 
            CancellationToken ct)
        {
            var command = new CreateOrderCommand(request.Ativo, request.Lado, request.Quantidade, request.Preco);

            var result = await orderService.ProcessAsync(command, ct);

            var response = new CreateOrderResponse(result.Success, result.CurrentExposure, result.ErrorMessage);

            return Ok(response);
        }
    }
}
