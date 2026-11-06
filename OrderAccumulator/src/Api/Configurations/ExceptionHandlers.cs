using Api.Contracts;
using Application.Persistence;
using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Configurations
{
    public static class ExceptionHandlers
    {
        public static void UseCustomExceptionHandlers(this WebApplication app)
        {
            app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
            {
                var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                var (status, message) = error switch
                {
                    DomainException => (
                        StatusCodes.Status400BadRequest,
                        error.Message),

                    ConcurrencyConflictException => (
                        StatusCodes.Status409Conflict,
                        error.Message),

                    _ => (
                        StatusCodes.Status500InternalServerError,
                        "Erro inesperado.")
                };

                context.Response.StatusCode = status;

                await context.Response.WriteAsJsonAsync(
                    CreateOrderResponse.Failure(message));
            }));
        }
    }
}
