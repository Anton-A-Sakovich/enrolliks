namespace Enrolliks.Persistence.Skills
{
    public interface ISkillValidator
    {
        SkillValidationErrors? Validate(Skill skill);
    }
}
