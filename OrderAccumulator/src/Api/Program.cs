using Api.Configurations;
using Api.Contracts;
using Api.Validators;
using Application;
using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
        options.InvalidModelStateResponseFactory = _ =>
            new BadRequestObjectResult(
                CreateOrderResponse.Failure(
                    "JSON da requisição inválido ou com tipos incorretos.")));

builder.Services.AddScoped<IValidator<CreateOrderRequest>, CreateOrderRequestValidator>();
builder.Services.AddOpenApi();

builder.Services.AddCustomCors();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

app.UseCustomExceptionHandlers();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseRouting();

app.UseCustomCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
