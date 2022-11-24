using AutoMapper;

namespace Enrolliks.Persistence.EntityFramework.Skills
{
    internal class SkillProfile : Profile
    {
        public SkillProfile()
        {
            CreateMap<Skill, SkillEntity>().ReverseMap();
        }
    }
}
