using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Msh.Api.Domain.Entities;
using Msh.Api.Infra.Identity;

namespace Msh.Api.Infra.Context;

public class MshDbContext : IdentityDbContext<ApplicationUser>
{
    public MshDbContext(DbContextOptions<MshDbContext> options) : base(options) { }

    // O nome da propriedade (UserRefreshTokens) será o nome da tabela no PostgreSQL
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Dica: Você pode configurar o tamanho das colunas aqui se quiser
        builder.Entity<UserRefreshToken>(entity =>
        {
            entity.Property(e => e.Token).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Token).IsUnique(); 
        });
    }
}