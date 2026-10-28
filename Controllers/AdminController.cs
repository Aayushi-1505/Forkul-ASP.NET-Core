using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;

namespace Forkful.Controllers;

// Classroom CRUD demonstration. Add authentication/authorization before production use.
[Route("admin/products")]
public class AdminController(AppDbContext db, IMemoryCache cache) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Manage products";
        return View(await db.Products.AsNoTracking().Include(p => p.Category)
            .OrderBy(p => p.Id).ToListAsync());
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create()
    {
        await LoadChoices();
        ViewData["Title"] = "Add a dish";
        return View("Form", new ProductFormViewModel());
    }

    [HttpPost("create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel form)
    {
        await ValidateChoices(form);
        if (!ModelState.IsValid)
        {
            await LoadChoices();
            ViewData["Title"] = "Add a dish";
            return View("Form", form);
        }

        db.Products.Add(new Product
        {
            Name = form.Name.Trim(), Slug = await UniqueSlug(form.Name),
            Description = form.Description.Trim(), Price = form.Price,
            ImageUrl = form.ImageUrl.Trim(), CategoryId = form.CategoryId,
            SupplierId = form.SupplierId, PrepMinutes = form.PrepMinutes,
            Rating = form.Rating, IsVegetarian = form.IsVegetarian,
            IsFeatured = form.IsFeatured, IsAvailable = form.IsAvailable
        });
        await db.SaveChangesAsync();
        cache.Remove("featured-products");
        TempData["Toast"] = "Dish added to the menu";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        await LoadChoices();
        ViewData["Title"] = "Edit a dish";
        return View("Form", new ProductFormViewModel
        {
            Id = product.Id, Name = product.Name, Description = product.Description,
            Price = product.Price, ImageUrl = product.ImageUrl, CategoryId = product.CategoryId,
            SupplierId = product.SupplierId, PrepMinutes = product.PrepMinutes,
            Rating = product.Rating, IsVegetarian = product.IsVegetarian,
            IsFeatured = product.IsFeatured, IsAvailable = product.IsAvailable
        });
    }

    [HttpPost("{id:int}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel form)
    {
        if (id != form.Id) return BadRequest();
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        await ValidateChoices(form);
        if (!ModelState.IsValid)
        {
            await LoadChoices();
            ViewData["Title"] = "Edit a dish";
            return View("Form", form);
        }

        product.Name = form.Name.Trim();
        product.Slug = await UniqueSlug(form.Name, id);
        product.Description = form.Description.Trim();
        product.Price = form.Price;
        product.ImageUrl = form.ImageUrl.Trim();
        product.CategoryId = form.CategoryId;
        product.SupplierId = form.SupplierId;
        product.PrepMinutes = form.PrepMinutes;
        product.Rating = form.Rating;
        product.IsVegetarian = form.IsVegetarian;
        product.IsFeatured = form.IsFeatured;
        product.IsAvailable = form.IsAvailable;
        await db.SaveChangesAsync();
        cache.Remove("featured-products");
        TempData["Toast"] = "Dish updated successfully";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        ViewData["Title"] = "Delete a dish";
        return View(product);
    }

    [HttpPost("{id:int}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        cache.Remove("featured-products");
        TempData["Toast"] = "Dish removed from the menu";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadChoices()
    {
        ViewBag.Categories = new SelectList(await db.Categories.AsNoTracking()
            .OrderBy(x => x.SortOrder).ToListAsync(), "Id", "Name");
        ViewBag.Suppliers = new SelectList(await db.Suppliers.AsNoTracking()
            .OrderBy(x => x.Name).ToListAsync(), "Id", "Name");
    }

    private async Task ValidateChoices(ProductFormViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.ImageUrl) ||
            !form.ImageUrl.StartsWith("/images/", StringComparison.Ordinal) || form.ImageUrl.Contains(".."))
            ModelState.AddModelError(nameof(form.ImageUrl), "Use a local image path beginning with /images/.");
        var category = await db.Categories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == form.CategoryId);
        if (category is null)
            ModelState.AddModelError(nameof(form.CategoryId), "Choose an existing category.");
        else if ((category.Slug == "non-veg") == form.IsVegetarian)
            ModelState.AddModelError(nameof(form.CategoryId),
                "Non-vegetarian dishes belong in Non-Veg; other categories are vegetarian.");
        if (!await db.Suppliers.AnyAsync(x => x.Id == form.SupplierId))
            ModelState.AddModelError(nameof(form.SupplierId), "Choose an existing supplier.");
    }

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
