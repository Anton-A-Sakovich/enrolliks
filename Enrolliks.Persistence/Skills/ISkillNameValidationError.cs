namespace Enrolliks.Persistence.Skills
{
    public interface ISkillNameValidationError
    {
        public record class Empty() : ISkillNameValidationError;

        public record class TooShort(int MinCharactersRequired) : ISkillNameValidationError;

        public record class TooLong(int MaxCharactersAllowed) : ISkillNameValidationError;

        public record class DisallowedLetter(string OffendingLetter) : ISkillNameValidationError;
    }
}
