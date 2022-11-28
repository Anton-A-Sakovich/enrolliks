using System.ComponentModel.DataAnnotations;

namespace Enrolliks.Web.Controllers.Skills
{
    public class CreateSkillModel
    {
        [Required]
        public required string Id { get; init; }

        [Required]
        public required string Name { get; init; }
    }
}
