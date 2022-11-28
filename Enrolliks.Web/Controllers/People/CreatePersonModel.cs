using System.ComponentModel.DataAnnotations;

namespace Enrolliks.Web.Controllers.People
{
    public class CreatePersonModel
    {
        [Required]
        public required string Name { get; init; }
    }
}
