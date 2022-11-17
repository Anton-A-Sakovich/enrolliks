using Enrolliks.Persistence.People;

namespace Enrolliks.Web.Controllers.People
{
    public class PersonValidationErrorsModel
    {
        public DiscriminatedUnionModel<INameValidationError>? Name { get; set; }
    }
}
