using Microsoft.AspNetCore.Mvc;

namespace Forkful.Controllers;

public class OrderController(AppDbContext db) : Controller
{
    [HttpGet("order/success/{code:guid}")]
    public async Task<IActionResult> Success(Guid code)
    {
        var order = await db.Orders.AsNoTracking().Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.PublicId == code);
        if (order is null) return NotFound();
        ViewData["Title"] = "Order confirmed";
        return View(order);
    }
}
