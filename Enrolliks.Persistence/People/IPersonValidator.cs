namespace Enrolliks.Persistence.People
{
    public interface IPersonValidator
    {
        PersonValidationErrors? Validate(Person person);
    }
}
