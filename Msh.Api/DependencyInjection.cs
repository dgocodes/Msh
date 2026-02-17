using System.Net.Http.Headers;
using Meilisearch;
using Microsoft.Extensions.Options;
using Msh.Api.Infra.Providers.Meili;

namespace Msh.Api;

public static class DependencyInjection
{
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