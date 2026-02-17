using Microsoft.Extensions.Caching.Hybrid;

namespace Msh.Api.Extensions;

public static class HybridCacheExtensions
{
    /// <summary>
    /// Cria opções de cache personalizadas para o HybridCache de forma simplificada.
    /// </summary>
    /// <param name="l1Minutes">Tempo de vida na memória local da API (Railway Instance)</param>
    /// <param name="l2Minutes">Tempo de vida no Redis compartilhado</param>
    public static HybridCacheEntryOptions CreateOptions(int l1Minutes, int l2Minutes)
    {
        return new HybridCacheEntryOptions
        {
            LocalCacheExpiration = TimeSpan.FromMinutes(l1Minutes),
            Expiration = TimeSpan.FromMinutes(l2Minutes)
        };
    }
}