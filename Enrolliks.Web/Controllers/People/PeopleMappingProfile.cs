using AutoMapper;
using Enrolliks.Persistence.People;

namespace Enrolliks.Web.Controllers.People
{
    public class PeopleMappingProfile : Profile
    {
        public PeopleMappingProfile()
        {
            CreateMap<CreatePersonModel, Person>();

            this.CreateDiscriminatedUnionMap<IPersonNameValidationError>();
            CreateMap<PersonValidationErrors, PersonValidationErrorsModel>();
        }
    }
}
