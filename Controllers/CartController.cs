using Microsoft.AspNetCore.Mvc;

namespace Forkful.Controllers;

[Route("cart")]
public class CartController(AppDbContext db, CartService cart,
    DishCustomizationService customizer) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Your cart";
        return View(await cart.GetSummaryAsync());
    }

    [HttpPost("add"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([FromForm] AddToCartRequest request)
    {
        if (!ModelState.IsValid) return BadRequest("Check your quantity or special instructions.");
        var product = await db.Products.AsNoTracking().Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsAvailable);
        if (product is null) return NotFound();
        if (!customizer.IsValidSpice(product, request.SpiceLevel) ||
            !customizer.TryResolveAddOns(product, request.AddOnCodes, out var selected))
            return BadRequest("One or more customization choices are not available for this dish.");

        var spice = customizer.NormalizeSpice(product, request.SpiceLevel);
        var instructions = string.IsNullOrWhiteSpace(request.SpecialInstructions)
            ? null : request.SpecialInstructions.Trim();
        cart.Add(product.Id, request.Quantity, spice,
            selected.Select(option => option.Code).ToList(), instructions);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { count = cart.Count, message = $"Custom {product.Name} added to your cart" });
        TempData["Toast"] = $"Custom {product.Name} added to your cart";
        return !string.IsNullOrEmpty(request.ReturnUrl) && Url.IsLocalUrl(request.ReturnUrl)
            ? LocalRedirect(request.ReturnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpPost("update"), ValidateAntiForgeryToken]
    public IActionResult Update(string lineId, int quantity)
    {
        cart.Update(lineId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("remove"), ValidateAntiForgeryToken]
    public IActionResult Remove(string lineId)
    {
        cart.Remove(lineId);
        TempData["Toast"] = "Item removed from your cart";
        return RedirectToAction(nameof(Index));
    }
}
