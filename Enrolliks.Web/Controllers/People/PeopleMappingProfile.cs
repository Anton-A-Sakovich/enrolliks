using AutoMapper;
using Enrolliks.Persistence.People;

namespace Enrolliks.Web.Controllers.People
{
    public class PeopleMappingProfile : Profile
    {
        public PeopleMappingProfile()
        {
            this.CreateDiscriminatedUnionMap<IPersonNameValidationError>();
            this.CreateDiscriminatedUnionMap<IGetAllPeopleResult>();
            this.CreateDiscriminatedUnionMap<ICreatePersonResult>();

            CreateMap<PersonValidationErrors, PersonValidationErrorsModel>();
        }
    }
}
