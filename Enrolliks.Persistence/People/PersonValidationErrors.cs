namespace Enrolliks.Persistence.People
{
    public record class PersonValidationErrors
    {
        public INameValidationError? Name { get; init; }
    }

    public interface INameValidationError
    {
        public record class Empty() : INameValidationError;

        public record class TooShort(int MinCharactersRequired) : INameValidationError;

        public record class TooLong(int MaxCharactersAllowed) : INameValidationError;
    }
}
