using Microsoft.AspNetCore.Identity;

namespace EShopOnWeb.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
