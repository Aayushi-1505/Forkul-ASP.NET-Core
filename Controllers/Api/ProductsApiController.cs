using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Forkful.Controllers.Api;

// Exercise 12: API CRUD uses the same EF Core provider as the MVC storefront.
[ApiController]
[Route("api/products")]
public class ProductsApiController(AppDbContext db, IMemoryCache cache) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok((await db.Products.AsNoTracking()
        .Include(x => x.Category).OrderBy(x => x.Id).ToListAsync()).Select(ToDto));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await db.Products.AsNoTracking().Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
        return product is null ? NotFound() : Ok(ToDto(product));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductWriteDto input)
    {
        if (!await ReferencesExist(input)) return BadRequest("Choose a valid category and supplier.");
        if (!await MatchesDietCategory(input)) return BadRequest("Non-vegetarian dishes must use the Non-Veg category; other categories are vegetarian.");
        if (!ValidImage(input.ImageUrl)) return BadRequest("ImageUrl must be a local /images/ path.");
        var product = new Product();
        Apply(product, input);
        product.Slug = await UniqueSlug(input.Name);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        cache.Remove("featured-products");
        product.Category = (await db.Categories.FindAsync(product.CategoryId))!;
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ToDto(product));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductWriteDto input)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        if (!await ReferencesExist(input)) return BadRequest("Choose a valid category and supplier.");
        if (!await MatchesDietCategory(input)) return BadRequest("Non-vegetarian dishes must use the Non-Veg category; other categories are vegetarian.");
        if (!ValidImage(input.ImageUrl)) return BadRequest("ImageUrl must be a local /images/ path.");
        Apply(product, input);
        product.Slug = await UniqueSlug(input.Name, id);
        await db.SaveChangesAsync();
        cache.Remove("featured-products");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        cache.Remove("featured-products");
        return NoContent();
    }

    private static object ToDto(Product p) => new
    {
        p.Id, p.Name, p.Slug, p.Description, p.Price, p.ImageUrl,
        Category = p.Category?.Name, p.CategoryId, p.SupplierId,
        p.PrepMinutes, p.Rating, p.IsVegetarian, p.IsFeatured, p.IsAvailable
    };

    private static void Apply(Product p, ProductWriteDto dto)
    {
        p.Name = dto.Name.Trim();
        p.Description = dto.Description.Trim();
        p.Price = dto.Price;
        p.ImageUrl = dto.ImageUrl.Trim();
        p.CategoryId = dto.CategoryId;
        p.SupplierId = dto.SupplierId;
        p.PrepMinutes = dto.PrepMinutes;
        p.Rating = dto.Rating;
        p.IsVegetarian = dto.IsVegetarian;
        p.IsFeatured = dto.IsFeatured;
        p.IsAvailable = dto.IsAvailable;
    }

    private Task<bool> ReferencesExist(ProductWriteDto dto) =>
        ValidateReferences(dto);

    private async Task<bool> ValidateReferences(ProductWriteDto dto) =>
        await db.Categories.AnyAsync(c => c.Id == dto.CategoryId) &&
        await db.Suppliers.AnyAsync(s => s.Id == dto.SupplierId);

    private async Task<bool> MatchesDietCategory(ProductWriteDto dto)
    {
        var slug = await db.Categories.Where(c => c.Id == dto.CategoryId)
            .Select(c => c.Slug).FirstAsync();
        return (slug == "non-veg") != dto.IsVegetarian;
    }

    private static bool ValidImage(string value) =>
        value.StartsWith("/images/", StringComparison.Ordinal) && !value.Contains("..");

    private async Task<string> UniqueSlug(string name, int? exceptId = null)
    {
        var basis = SlugHelper.From(name);
        var slug = basis;
        var suffix = 2;
        while (await db.Products.AnyAsync(x => x.Slug == slug && x.Id != exceptId))
            slug = $"{basis}-{suffix++}";
        return slug;
    }
}

public class ProductWriteDto
{
    [Required, StringLength(100, MinimumLength = 3)] public string Name { get; set; } = "";
    [Required, StringLength(300, MinimumLength = 10)] public string Description { get; set; } = "";
    [Range(1, 10000)] public decimal Price { get; set; }
    [Required, StringLength(250)] public string ImageUrl { get; set; } = "";
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
    [Range(1, int.MaxValue)] public int SupplierId { get; set; }
    [Range(1, 120)] public int PrepMinutes { get; set; } = 25;
    [Range(0, 5)] public decimal Rating { get; set; } = 4.8m;
    public bool IsVegetarian { get; set; } = true;
    public bool IsFeatured { get; set; }
    public bool IsAvailable { get; set; } = true;
}
