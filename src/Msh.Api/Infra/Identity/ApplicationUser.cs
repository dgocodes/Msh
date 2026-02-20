using Microsoft.AspNetCore.Identity;
using Msh.Api.Core.Entities;

namespace Msh.Api.Infra.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    // The "secret sauce": The ID that your ERP recognizes
    public string ErpId { get; set; } = string.Empty;

    // Type identifier to facilitate quick filtering
    public EUserType Type { get; set; }
}
