using Microsoft.AspNetCore.Mvc;

namespace ProductHub.Web.Controllers.MVC
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "ProductsPage");
        }
    }
}