using System.ComponentModel.DataAnnotations;

namespace Enrolliks.Web.Controllers.Skills
{
    public class CreateSkillConflictModel
    {
        [Required]
        public required string PropertyName { get; init; }
    }
}
