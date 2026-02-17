using Msh.Api;
using Msh.Api.Endpoints;
using Msh.Api.Middleware;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddOpenApi()
                .AddCustomJsonOptions()
                .AddCustomCompression()
                .AddMeiliSearchServices(configuration)
                .AddCustomCors(configuration)
                .AddApplicationServices(configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var name = configuration.GetValue<string>("Cors:Name") ?? "NextJsApp";

app.UseCors(name);
app.UseResponseCompression();
app.UseExceptionHandler();

app.MapProductEndpoints();
app.MapHealthChecks("/health");

app.Run();
