using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.Controllers.Home
{
    public class HomeController : PageController
    {
        public IActionResult Index()
        {
            return Page();
        }
    }
}
