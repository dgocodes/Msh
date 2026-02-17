using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Meilisearch;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using Msh.Api.Domain.Builders;
using Msh.Api.Domain.Interfaces.Builders;
using Msh.Api.Domain.Interfaces.Providers;
using Msh.Api.Infra.Providers.Meili;

namespace Msh.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(opt =>
        {
            opt.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            opt.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opt.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            opt.SerializerOptions.WriteIndented = false;
            opt.SerializerOptions.PropertyNameCaseInsensitive = true;
        });

        return services;
    }

    public static IServiceCollection AddCustomCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(opt =>
        {
            opt.EnableForHttps = true;
            opt.Providers.Add<BrotliCompressionProvider>();
            opt.Providers.Add<GzipCompressionProvider>();
        });

        services.Configure<BrotliCompressionProviderOptions>(opt =>
        {
            opt.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(opt =>
        {
            opt.Level = CompressionLevel.Fastest;
        });

        return services;
    }

    public static IServiceCollection AddCustomCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        var name = configuration.GetValue<string>("Cors:Name") ?? "NextJsApp";

        services.AddCors(opt =>
            opt.AddPolicy(name, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .WithExposedHeaders("X-Correlation-ID");
            })
        );

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISearchProvider, MeiliSearchProvider>();
        services.AddSingleton<IFacetBuilder, FacetBuilder>();

        return services;
    }

    public static IServiceCollection AddMeiliSearchServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<MeiliSearchConfiguration>()
              .Bind(configuration.GetSection(MeiliSearchConfiguration.SectionName))
              .ValidateDataAnnotations()
              .ValidateOnStart();

        services.AddHttpClient(MeiliSearchConfiguration.ClientName, (serviceProvider, client) =>
        {
            var configMeilisearch = serviceProvider.GetRequiredService<IOptions<MeiliSearchConfiguration>>().Value;

            client.BaseAddress = new Uri(configMeilisearch.Url);

            if (!string.IsNullOrEmpty(configMeilisearch.ApiKey))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", configMeilisearch.ApiKey);

            client.Timeout = TimeSpan.FromSeconds(4);
        }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            // Mantém as conexões vivas por 5 minutos para evitar o overhead de novo aperto de mão TCP
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),

            // Define o limite de conexões simultâneas para o MeiliSearch
            MaxConnectionsPerServer = 30,

            // Ativa o Keep Alive para evitar que o container feche a conexão ociosa
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),

            ConnectTimeout = TimeSpan.FromSeconds(4) // Se não conectar em 2s, cancela
        });

        services.AddSingleton(sp =>
        {
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient(MeiliSearchConfiguration.ClientName);

            return new MeilisearchClient(httpClient);
        });


        return services;
    }
}