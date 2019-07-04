namespace to.frontend.Controllers
{
    using System.Diagnostics;

    using Microsoft.AspNetCore.Mvc;

    using Models;

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier});
        }
    }
}