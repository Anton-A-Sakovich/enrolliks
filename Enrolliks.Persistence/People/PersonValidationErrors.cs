namespace Enrolliks.Persistence.People
{
    public record class PersonValidationErrors
    {
        public IPersonNameValidationError? Name { get; init; }
    }
}
