using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Meilisearch;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Msh.Api.Domain.Builders;
using Msh.Api.Domain.Interfaces.Builders;
using Msh.Api.Domain.Interfaces.Providers;
using Msh.Api.Infra.Providers.Meili;

namespace Msh.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddCustomAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);

        services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true
            };
        });

        services.AddAuthorization();
        return services;
    }

    //public static IServiceCollection AddCustomOpenApi(this IServiceCollection services)
    //{
    //    services.AddOpenApi(options => {
    //        options.AddDocumentTransformer((document, context, cancellationToken) => {
    //            var scheme = new OpenApiSecurityScheme
    //            {
    //                Type = SecuritySchemeType.Http,
    //                Name = "Authorization",
    //                In = ParameterLocation.Header,
    //                Scheme = "bearer",
    //                BearerFormat = "JWT"
    //            };
    //            document.Components ??= new OpenApiComponents();
    //            document.Components.SecuritySchemes.Add("Bearer", scheme);
    //            document.Security = [new OpenApiSecurityRequirement {
    //                [new OpenApiSecurityScheme {
    //                    Reference = new OpenApiReference { Type = OpenApiReferenceType.SecurityScheme, Id = "Bearer" }
    //                }] = []
    //            }];
    //            return Task.CompletedTask;
    //        });
    //    });
    //    return services;
    //}

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
            //opt.Providers.Add<BrotliCompressionProvider>();
            opt.Providers.Add<GzipCompressionProvider>();
        });

        //services.Configure<BrotliCompressionProviderOptions>(opt =>
        //{
        //    opt.Level = CompressionLevel.Fastest;
        //});

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
                      .AllowCredentials();
                      //.WithExposedHeaders("X-Correlation-ID");
            })
        );

        return services;
    }

    public static IServiceCollection AddCustomRateLimiting(
      this IServiceCollection services,
      IConfiguration configuration)
    {
        if (configuration.GetValue("Features:UseRateLimit", true))
        {
            int globalLimit = configuration.GetValue("RateLimit:Global", 500);
            int endpointLimit = configuration.GetValue("RateLimit:SearchEndpoint", 250);

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
                {
                    // Ignora o limite para o endpoint de health check
                    if (ctx.Request.Path.StartsWithSegments("/health"))
                    {
                        return RateLimitPartition.GetNoLimiter("health");
                    }

                    var key = ctx.User.Identity?.Name
                              ?? ctx.Connection.RemoteIpAddress?.ToString()
                              ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(key, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = globalLimit,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0 // Não permite enfileirar requisições, rejeita imediatamente quando o limite é atingido
                        });
                });

                options.AddPolicy("buscas", ctx =>
                {
                    var key = ctx.User.Identity?.Name
                              ?? ctx.Connection.RemoteIpAddress?.ToString()
                              ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(key, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = endpointLimit,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
            });
        }

        return services;
    }


    public static IServiceCollection AddCustomHybridCacheRedis(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue("Features:UseRedis", true))
        {
            // 1. Configura a conexão base com o Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "MshCache-";
            });
        }

        services.AddHybridCache(options =>
        {
            // Define o limite de tamanho do que pode ser cacheado
            options.MaximumPayloadBytes = 3 * 1024 * 1024; // 3 MB
            options.MaximumKeyLength = 250;

            // Define o tempo padrão de expiração
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10), //Tempo total de expiração (tanto para cache local quanto para Redis)
                LocalCacheExpiration = TimeSpan.FromMinutes(5) // Tempo de expiração para o cache local (pode ser menor que o tempo total para reduzir a chance de cache stale)
            };
        });

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

        services.PostConfigure<MeiliSearchConfiguration>(options =>
        {
            if (options.Configuration.Facets != null)
            {
                options.Configuration.Facets = options.Configuration.Facets.OrderBy(f => f.Position).ToList();
            }
        });

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