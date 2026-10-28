using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Forkful.ViewModels;

public class HomeViewModel
{
    public IReadOnlyList<Product> Featured { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public GreetingViewModel Greeting { get; set; } = TimeGreeting.Today();
}

public class MenuViewModel
{
    public IReadOnlyList<Product> Products { get; set; } = Array.Empty<Product>();
    public IReadOnlyList<Category> Categories { get; set; } = Array.Empty<Category>();
    public string? Category { get; set; }
    public string? Search { get; set; }
    public string Sort { get; set; } = "popular";
    public string Diet { get; set; } = "all";
    public int TotalProducts { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int TotalPages => Math.Max(1, (TotalProducts + PageSize - 1) / PageSize);
    public int FirstResult => TotalProducts == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int LastResult => Math.Min(TotalProducts, Page * PageSize);
}

public record GreetingViewModel(string Title, string Subtitle, string Image, string DayPart);

public class AddToCartRequest
{
    [Range(1, int.MaxValue)] public int ProductId { get; set; }
    [Range(1, 20)] public int Quantity { get; set; } = 1;
    public string? SpiceLevel { get; set; }
    public List<string> AddOnCodes { get; set; } = new();
    [StringLength(200)] public string? SpecialInstructions { get; set; }
    public string? ReturnUrl { get; set; }
}

public class CartLine
{
    public string LineId { get; set; } = Guid.NewGuid().ToString("N");
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string SpiceLevel { get; set; } = "None";
    public List<string> AddOnCodes { get; set; } = new();
    public string? SpecialInstructions { get; set; }
}

public class CartItemViewModel
{
    public string LineId { get; set; } = "";
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public string SpiceLevel { get; set; } = "None";
    public List<AddOnOption> AddOns { get; set; } = new();
    public string? SpecialInstructions { get; set; }
    public decimal UnitPrice => Product.Price + AddOns.Sum(option => option.Price);
    public decimal LineTotal => UnitPrice * Quantity;
}

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();
    public int Count => Items.Sum(i => i.Quantity);
    public decimal Subtotal => Items.Sum(i => i.LineTotal);
    public decimal DeliveryFee => Items.Count == 0 || Subtotal >= 599 ? 0 : 39;
    public decimal Total => Subtotal + DeliveryFee;
    public decimal AwayFromFreeDelivery => Math.Max(0, 599 - Subtotal);
}

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Please enter your name."), StringLength(100, MinimumLength = 2)]
    [Display(Name = "Full name")]
    public string CustomerName { get; set; } = "";

    [Required(ErrorMessage = "Please enter your email."), EmailAddress, StringLength(160)]
    [Display(Name = "Email address")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Please enter your mobile number.")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
    [Display(Name = "Mobile number")]
    public string Phone { get; set; } = "";

    [Required, RegularExpression("^(Delivery|Pickup)$", ErrorMessage = "Choose delivery or pickup.")]
    public string DeliveryType { get; set; } = "Delivery";

    [StringLength(300)]
    [Display(Name = "Delivery address")]
    public string? Address { get; set; }

    [StringLength(300)]
    [Display(Name = "Order notes")]
    public string? Notes { get; set; }

    [Required, RegularExpression("^(Cash on delivery|Pay at pickup)$", ErrorMessage = "Choose a valid payment option.")]
    public string PaymentMethod { get; set; } = "Cash on delivery";

    // Display-only; never trust prices submitted from the browser.
    [ValidateNever]
    public CartViewModel Cart { get; set; } = new();
}

public class ProductFormViewModel
{
    public int Id { get; set; }
    [Required, StringLength(100, MinimumLength = 3)] public string Name { get; set; } = "";
    [Required, StringLength(300, MinimumLength = 10)] public string Description { get; set; } = "";
    [Range(1, 10000, ErrorMessage = "Enter a price between ₹1 and ₹10,000.")] public decimal Price { get; set; }
    [Required, StringLength(250)] [Display(Name = "Image path")]
    public string ImageUrl { get; set; } = "/images/menu/pasta.jpg";
    [Range(1, int.MaxValue, ErrorMessage = "Choose a category.")] public int CategoryId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Choose a supplier.")] public int SupplierId { get; set; }
    [Range(1, 120)] [Display(Name = "Prep time (minutes)")] public int PrepMinutes { get; set; } = 25;
    [Range(0, 5)] public decimal Rating { get; set; } = 4.8m;
    public bool IsVegetarian { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsAvailable { get; set; } = true;
}
