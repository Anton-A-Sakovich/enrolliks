using Enrolliks.Persistence;

namespace Enrolliks.Web.Controllers.People
{
    public class PersonValidationErrorsModel
    {
        public DiscriminatedUnionModel<INameValidationError>? Name { get; set; }
    }
}
