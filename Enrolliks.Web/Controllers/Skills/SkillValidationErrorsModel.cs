using Enrolliks.Persistence.Skills;

namespace Enrolliks.Web.Controllers.Skills
{
    public class SkillValidationErrorsModel
    {
        public DiscriminatedUnionModel<ISkillNameValidationError>? Name { get; init; }
    }
}
