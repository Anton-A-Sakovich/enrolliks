using System.Text.Json;

namespace Enrolliks.Web.Controllers
{
    internal static class JsonExtensions
    {
        public static string ToPropertyName(this string name, JsonSerializerOptions options)
        {
            return options.PropertyNamingPolicy?.ConvertName(name) ?? name;
        }
    }
}
