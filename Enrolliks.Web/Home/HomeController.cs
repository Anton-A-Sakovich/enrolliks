using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.Home
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
