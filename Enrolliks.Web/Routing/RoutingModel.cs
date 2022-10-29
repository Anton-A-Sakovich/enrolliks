namespace Enrolliks.Web.Routing
{
    public class RoutingModel
    {
        public RoutingModel(string route)
        {
            Route = route;
        }

        public string Route { get; init; }
    }
}
