using System.ComponentModel.DataAnnotations;

namespace Enrolliks.Web.Controllers.Skills
{
    public class UpdateSkillModel
    {
        public string Id { get; set; } = null!;

        [Required]
        public required string Name { get; init; }
    }
}
