using Microsoft.AspNetCore.Mvc;

namespace Forkful.Controllers;

[Route("menu")]
public class MenuController(AppDbContext db) : Controller
{
    private const int PageSize = 12;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? category, string? q, string sort = "popular", string diet = "all", int page = 1)
    {
        diet = diet is "veg" or "nonveg" ? diet : "all";
        sort = sort is "price-low" or "price-high" or "name" ? sort : "popular";
        var query = db.Products.AsNoTracking().Include(p => p.Category).Where(p => p.IsAvailable);
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category.Slug == category);
        if (diet == "veg") query = query.Where(p => p.IsVegetarian);
        if (diet == "nonveg") query = query.Where(p => !p.IsVegetarian);
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.Name.Contains(q) || p.Description.Contains(q));
        }

        // Count the filtered set before paging, then order with a stable ID tie-breaker.
        var totalProducts = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (totalProducts + PageSize - 1) / PageSize));
        var ordered = sort switch
        {
            // SQLite cannot ORDER BY decimal directly; casting to double also works with MySQL.
            "price-low" => query.OrderBy(p => (double)p.Price).ThenBy(p => p.Id),
            "price-high" => query.OrderByDescending(p => (double)p.Price).ThenBy(p => p.Id),
            "name" => query.OrderBy(p => p.Name).ThenBy(p => p.Id),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => (double)p.Rating).ThenBy(p => p.Id)
        };

        ViewData["Title"] = "The menu";
        return View(new MenuViewModel
        {
            Products = await ordered.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
            TotalProducts = totalProducts, Page = page, PageSize = PageSize,
            Categories = await db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ToListAsync(),
            Category = category, Search = q, Sort = sort, Diet = diet
        });
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var product = await db.Products.AsNoTracking().Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsAvailable);
        if (product is null) return NotFound();
        ViewData["Title"] = product.Name;
        return View(product);
    }
}
