using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Msh.Api;
using Msh.Api.Endpoints;
using Msh.Api.Endpoints.Admin;
using Msh.Api.Infra.Context;
using Msh.Api.Infra.Context.Seed;
using Msh.Api.Infra.Identity;
using Msh.Api.Infra.Security;
using Msh.Api.Middleware;
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

// Endpoints
app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapAdminUsersEndpoints();
app.MapGet("/seguro", () => "Autenticado!").RequireAuthorization();

await DbInitializer.SeedAdminUser(app);

app.Run();