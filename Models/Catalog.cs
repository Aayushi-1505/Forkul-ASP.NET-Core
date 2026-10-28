using System.ComponentModel.DataAnnotations;

namespace Forkful.Models;

public class Category
{
    public int Id { get; set; }
    [Required, StringLength(60)] public string Name { get; set; } = "";
    [Required, StringLength(70)] public string Slug { get; set; } = "";
    [StringLength(200)] public string ImageUrl { get; set; } = "";
    public int SortOrder { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Supplier
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, StringLength(100)] public string City { get; set; } = "";
    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Product
{
    public int Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, StringLength(120)] public string Slug { get; set; } = "";
    [Required, StringLength(300)] public string Description { get; set; } = "";
    [Range(1, 10000)] public decimal Price { get; set; }
    [Required, StringLength(250)] public string ImageUrl { get; set; } = "";
    public bool IsVegetarian { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsAvailable { get; set; } = true;
    [Range(1, 120)] public int PrepMinutes { get; set; } = 25;
    [Range(0, 5)] public decimal Rating { get; set; } = 4.8m;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
}
