using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Msh.Api;
using Msh.Api.Core.Commom;
using Msh.Api.Endpoints;
using Msh.Api.Infra.Context;
using Msh.Api.Infra.Context.Seed;
using Msh.Api.Infra.Errors;
using Msh.Api.Infra.Identity;
using Msh.Api.Infra.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddOpenApi()
                .AddCustomAuth(configuration)
                .AddCustomJsonOptions()
                .AddCustomCompression()
                .AddMeiliSearchServices(configuration)
                .AddCustomCors(configuration)
                .AddApplicationServices(configuration)
                .AddCustomRateLimiting(configuration)
                .AddCustomHybridCacheRedis(configuration);

builder.Services.AddDbContext<MshDbContext>(opt => opt.UseNpgsql(configuration.GetConnectionString("Postgres")));
builder.Services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<MshDbContext>()
                .AddSignInManager();

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var handlers = typeof(Program).Assembly.GetTypes()
    .Where(t => t.IsClass && t.Name.EndsWith("Handler"));

foreach (var handler in handlers)
{
    builder.Services.AddScoped(handler);
}

var app = builder.Build();

app.UseExceptionHandler();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseCors(configuration.GetValue<string>("Cors:Name") ?? "NextJsApp");
app.UseResponseCompression();

if (configuration.GetValue("Features:UseRateLimit", true)) app.UseRateLimiter();

app.MapProductEndpoints();

await DbInitializer.SeedAdminUser(app);

//Inicializa os endpoints de forma dinâmica, buscando todas as classes que implementam IEndpoint e chamando seu método MapEndpoint
var endpointTypes = typeof(Program).Assembly.GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && typeof(IEndpoint).IsAssignableFrom(t));

foreach (var type in endpointTypes)
{
    var endpoint = (IEndpoint)Activator.CreateInstance(type)!;
    endpoint.MapEndpoint(app);
}

app.Run();