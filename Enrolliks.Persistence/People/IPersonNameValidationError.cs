namespace Enrolliks.Persistence.People
{
    public interface IPersonNameValidationError
    {
        public record class Empty() : IPersonNameValidationError;

        public record class TooShort(int MinCharactersRequired) : IPersonNameValidationError;

        public record class TooLong(int MaxCharactersAllowed) : IPersonNameValidationError;

        public record class StartsWithSpace() : IPersonNameValidationError;

        public record class DisallowedLetter(string OffendingLetter) : IPersonNameValidationError;
    }
}
