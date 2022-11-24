using AutoMapper;

namespace Enrolliks.Persistence.EntityFramework.People
{
    internal class PersonProfile : Profile
    {
        public PersonProfile()
        {
            CreateMap<Person, PersonEntity>().ReverseMap();
        }
    }
}
