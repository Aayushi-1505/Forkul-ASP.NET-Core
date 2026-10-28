using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Forkful.Controllers;

public class HomeController(AppDbContext db, IMemoryCache cache) : Controller
{
    // Exercise 2: the home action deliberately returns the named MyView view.
    public ViewResult Index()
    {
        var featured = cache.GetOrCreate("featured-products", entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return db.Products.AsNoTracking().Include(p => p.Category)
                .Where(p => p.IsFeatured && p.IsAvailable).OrderBy(p => p.Id).Take(4).ToList();
        }) ?? new List<Product>();

        ViewData["Title"] = "Food that makes your day"; // Exercise 13: ViewData
        ViewData["College"] = "Asha M. Tarsadia Institute of Computer Science and Technology";
        ViewBag.Greeting = TimeGreeting.Today().Title;  // Exercise 13: ViewBag
        return View("MyView", new HomeViewModel
        {
            Featured = featured,
            Categories = db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ToList(),
            Greeting = TimeGreeting.Today()
        });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
