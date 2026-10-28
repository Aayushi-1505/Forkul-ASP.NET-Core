using Microsoft.AspNetCore.Mvc;

namespace Forkful.Controllers;

public class GreetingsController : Controller
{
    [HttpGet("greetings")]
    public IActionResult Index()
    {
        ViewData["Title"] = "A greeting for you";
        return View("Index", TimeGreeting.Today());
    }
}
