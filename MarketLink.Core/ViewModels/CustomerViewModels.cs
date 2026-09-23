using System.ComponentModel.DataAnnotations;
using MarketLink.Core.Entities;

namespace MarketLink.Core.ViewModels;

public class CustomerProfileViewModel
{
    [Required(ErrorMessage = "First name is required")]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "About Me")]
    public string? Bio { get; set; }

    [Display(Name = "Default City")]
    public string? DefaultCity { get; set; }

    public string? ProfileImageUrl { get; set; }

    public DateTime MemberSince { get; set; } = DateTime.UtcNow;
    public int TotalOrders { get; set; }
    public int TotalFavorites { get; set; }
    public int TotalAddresses { get; set; }
    public IEnumerable<CustomerAddress> Addresses { get; set; } = [];
}

public class CustomerAddressViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Label is required (e.g. Home, Work)")]
    public string Label { get; set; } = "Home";

    [Required(ErrorMessage = "Street address is required")]
    [Display(Name = "Address Line 1")]
    public string AddressLine1 { get; set; } = string.Empty;

    [Display(Name = "Address Line 2 (Optional)")]
    public string? AddressLine2 { get; set; }

    [Required(ErrorMessage = "City is required")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "State / Province is required")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postal / ZIP Code is required")]
    [Display(Name = "Postal Code")]
    public string PostalCode { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}
