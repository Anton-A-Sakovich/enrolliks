using Microsoft.AspNetCore.Mvc;

namespace Enrolliks.Web.Controllers
{
    public class PageController : Controller
    {
        protected IActionResult Page(object? data = null, string? route = null)
        {
            if (route is null)
            {
                var actionDescriptor = ControllerContext.ActionDescriptor;
                route = $"{actionDescriptor.ControllerName}/{actionDescriptor.ActionName}";
            }

            return View("~/Controllers/Page.cshtml", new PageModel(route) { Data = data });
        }
    }
}
