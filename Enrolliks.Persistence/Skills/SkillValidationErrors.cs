namespace Enrolliks.Persistence.Skills
{
    public record class SkillValidationErrors
    {
        public ISkillNameValidationError? Name { get; set; }
    }
}
