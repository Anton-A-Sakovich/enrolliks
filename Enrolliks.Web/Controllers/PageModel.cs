namespace Enrolliks.Web.Controllers
{
    public class PageModel
    {
        public PageModel(string route)
        {
            Route = route;
        }

        public object? Data { get; set; }

        public string Route { get; init; }
    }
}
