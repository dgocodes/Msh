using System.Text.Json;
using System.Text.Json.Serialization;
using Meilisearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using Msh.Api;
using Msh.Api.Domain.Builders;
using Msh.Api.Domain.Contracts.Search;
using Msh.Api.Domain.Interfaces.Builders;
using Msh.Api.Domain.Interfaces.Providers;
using Msh.Api.Infra.Providers.Meili;
using Msh.Api.Middleware;
using Scalar.AspNetCore;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opt.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddResponseCompression(opt =>
{
    opt.EnableForHttps = true;
    opt.Providers.Add<BrotliCompressionProvider>();
    opt.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddMeiliSearchServices(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(opt =>
    opt.AddPolicy("NextJsApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("X-Correlation-ID");
    })
);


builder.Services.AddScoped<ISearchProvider, MeiliSearchProvider>();
builder.Services.AddSingleton<IFacetBuilder, FacetBuilder>();

//ThreadPool.SetMinThreads(200, 200);

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("NextJsApp");
//app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseExceptionHandler();

app.MapGet("/products", async (
    ISearchProvider provider,
    CancellationToken cancellationToken,
    [AsParameters] SearchProductRequest request) =>
{
    var result = await provider.SearchAsync(request, cancellationToken);
    return TypedResults.Ok(result);
})
   .AddOpenApiOperationTransformer((opperation, context, ct) =>
   {
       opperation.Summary = "Search Products";
       opperation.Description = "Searches for products based on a query and optional filters on provider Meilisearch.";

       return Task.CompletedTask;
   });

app.MapHealthChecks("/health");

app.Run();
