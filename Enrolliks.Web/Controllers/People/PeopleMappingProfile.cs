using AutoMapper;
using Enrolliks.Persistence.People;

namespace Enrolliks.Web.Controllers.People
{
    public class PeopleMappingProfile : Profile
    {
        public PeopleMappingProfile()
        {
            this.CreateDiscriminatedUnionMap<INameValidationError>();
            this.CreateDiscriminatedUnionMap<IGetAllPeopleResult>();
            this.CreateDiscriminatedUnionMap<ICreatePersonResult>();

            CreateMap<PersonValidationErrors, PersonValidationErrorsModel>();
        }
    }
}
