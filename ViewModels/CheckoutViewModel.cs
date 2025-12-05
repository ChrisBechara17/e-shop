using System.ComponentModel.DataAnnotations;

namespace EShopOnWeb.ViewModels;

public class CheckoutViewModel
{
    [Required]
    [Display(Name = "Full Name")]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Email Address")]
    public string CustomerEmail { get; set; } = string.Empty;
}
