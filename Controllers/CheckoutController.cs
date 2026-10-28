using Microsoft.AspNetCore.Mvc;

namespace Forkful.Controllers;

[Route("checkout")]
public class CheckoutController(AppDbContext db, CartService cart) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var summary = await cart.GetSummaryAsync();
        if (summary.Items.Count == 0) return RedirectToAction("Index", "Cart");
        ViewData["Title"] = "Checkout";
        return View(new CheckoutViewModel { Cart = summary });
    }

    [HttpPost(""), ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        model.Cart = await cart.GetSummaryAsync();

        // Exercise 14: explicit server-side validation complements the attributes above.
        if (model.Cart.Items.Count == 0)
            ModelState.AddModelError("", "Your cart is empty. Add something delicious first.");
        if (model.DeliveryType == "Delivery" && (model.Address?.Trim().Length ?? 0) < 10)
            ModelState.AddModelError(nameof(model.Address), "Enter a delivery address of at least 10 characters.");
        if (model.DeliveryType == "Delivery" && model.PaymentMethod != "Cash on delivery")
            ModelState.AddModelError(nameof(model.PaymentMethod), "Delivery orders use cash on delivery in this demo.");
        if (model.DeliveryType == "Pickup" && model.PaymentMethod != "Pay at pickup")
            ModelState.AddModelError(nameof(model.PaymentMethod), "Pickup orders are paid at pickup in this demo.");

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Checkout";
            return View(model);
        }

        var fee = model.DeliveryType == "Pickup" ? 0 : model.Cart.DeliveryFee;
        var order = new Order
        {
            CustomerName = model.CustomerName.Trim(), Email = model.Email.Trim(),
            Phone = model.Phone.Trim(), Address = model.DeliveryType == "Delivery" ? model.Address?.Trim() : null,
            Notes = model.Notes?.Trim(), DeliveryType = model.DeliveryType,
            PaymentMethod = model.PaymentMethod, Subtotal = model.Cart.Subtotal,
            DeliveryFee = fee, Total = model.Cart.Subtotal + fee,
            Items = model.Cart.Items.Select(i => new OrderItem
            {
                ProductId = i.Product.Id, ProductName = i.Product.Name,
                UnitPrice = i.UnitPrice, Quantity = i.Quantity,
                SpiceLevel = i.SpiceLevel == "None" ? null : i.SpiceLevel,
                AddOns = i.AddOns.Count == 0 ? null : string.Join(", ", i.AddOns.Select(a => a.Name)),
                SpecialInstructions = i.SpecialInstructions
            }).ToList()
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        cart.Clear();
        TempData["Toast"] = "Your order is on its way to the kitchen!";
        return RedirectToAction("Success", "Order", new { code = order.PublicId });
    }
}
