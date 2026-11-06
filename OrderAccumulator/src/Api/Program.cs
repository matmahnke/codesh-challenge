using Api.Contracts;
using Api.Validators;
using Application;
using Application.Persistence;
using Domain.Exceptions;
using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

const string FrontendCorsPolicy = "Frontend";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    // Erros de binding (corpo ausente, JSON malformado, tipo incorreto) no formato de resposta do contrato.
    .ConfigureApiBehaviorOptions(options => options.InvalidModelStateResponseFactory = _ =>
        new BadRequestObjectResult(
            CreateOrderResponse.Failure("JSON da requisição inválido ou com tipos incorretos.")));
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    options.AddPolicy(FrontendCorsPolicy, policy => policy
        .WithOrigins(origins)
        .AllowAnyHeader()
        .WithMethods("POST"));
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, message) = error switch
    {
        DomainException => (StatusCodes.Status400BadRequest, error.Message),
        ConcurrencyConflictException => (StatusCodes.Status409Conflict, error.Message),
        _ => (StatusCodes.Status500InternalServerError, "Erro inesperado.")
    };

    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(CreateOrderResponse.Failure(message));
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// CORS antes do redirect, para o preflight (OPTIONS) não ser redirecionado.
app.UseCors(FrontendCorsPolicy);

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
