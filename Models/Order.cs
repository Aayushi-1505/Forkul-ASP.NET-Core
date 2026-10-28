using System.ComponentModel.DataAnnotations;

namespace Forkful.Models;

public class Order
{
    public int Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    [Required, StringLength(100)] public string CustomerName { get; set; } = "";
    [Required, EmailAddress, StringLength(160)] public string Email { get; set; } = "";
    [Required, StringLength(20)] public string Phone { get; set; } = "";
    [StringLength(300)] public string? Address { get; set; }
    [StringLength(300)] public string? Notes { get; set; }
    [Required, StringLength(20)] public string DeliveryType { get; set; } = "Delivery";
    [Required, StringLength(30)] public string PaymentMethod { get; set; } = "Cash on delivery";
    [Required, StringLength(30)] public string Status { get; set; } = "Received";
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    [Required, StringLength(100)] public string ProductName { get; set; } = "";
    // The final unit price already includes any selected extras.
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    [StringLength(20)] public string? SpiceLevel { get; set; }
    [StringLength(240)] public string? AddOns { get; set; }
    [StringLength(200)] public string? SpecialInstructions { get; set; }
}
