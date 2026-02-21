using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Msh.Api.Core.Entities;
using Msh.Api.Infra.Identity;

namespace Msh.Api.Infra.Context;

public class MshDbContext : IdentityDbContext<ApplicationUser>
{
    public MshDbContext(DbContextOptions<MshDbContext> options) : base(options) { }

    // O nome da propriedade (UserRefreshTokens) será o nome da tabela no PostgreSQL
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Dica: Você pode configurar o tamanho das colunas aqui se quiser
        builder.Entity<UserRefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Token).IsUnique();

            // Configura o relacionamento: Um usuário tem muitos tokens
            entity.HasOne(d => d.User)
                  .WithMany(p => p.UserRefreshTokens)
                  .HasForeignKey(d => d.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // Se deletar o usuário, limpa os tokens dele
        });

        builder.Entity<Order>(entity => {
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(o => o.Payment).WithOne(p => p.Order).HasForeignKey<Payment>(p => p.OrderId);
        });

        builder.Entity<OrderItem>(entity => {
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
        });

        builder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);
            // Garante que não teremos dois carrinhos para o mesmo usuário
            entity.HasIndex(c => c.UserId).IsUnique();

            entity.HasMany(c => c.Items)
                  .WithOne()
                  .HasForeignKey(i => i.CartId) // Se você adicionar CartId no CartItem
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}