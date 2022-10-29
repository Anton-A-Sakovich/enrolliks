using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.Routing
{
    public class RoutingController : Controller
    {
        [HttpGet("{area=home}/{page=index}")]
        public IActionResult Index(string area, string page)
        {
            var routingModel = new RoutingModel(area + "/" + page);

            return View("~/Routing/Index.cshtml", routingModel);
        }
    }
}
