using Microsoft.AspNetCore.Mvc;

namespace ProductHub.Web.Controllers.MVC
{
    public class ProductsPageController : Controller
    {
        [HttpGet("products")]
        public IActionResult Index()
        {
            return View();
        }
    }
}