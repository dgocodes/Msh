using Microsoft.AspNetCore.Identity;
using Msh.Api.Domain.Entities;
using Msh.Api.Infra.Identity;

namespace Msh.Api.Infra.Context.Seed;

public static class DbInitializer
{
    public static async Task SeedAdminUser(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Criar a Role de Admin se não existir
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2. Verificar se o Admin já existe para não duplicar
        var adminEmail = "admin@msh.com.br";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador Sistema",
                ErpId = "", // ID padrão para o admin
                Type = EUserType.Admin,
                EmailConfirmed = true
            };

            // 3. Criar o usuário com uma senha segura
            var result = await userManager.CreateAsync(adminUser, "Admin@123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}