using AutoMapper;
using Enrolliks.Persistence.Skills;

namespace Enrolliks.Web.Controllers.Skills
{
    public class SkillsMappingProfile : Profile
    {
        public SkillsMappingProfile()
        {
            CreateMap<CreateSkillModel, Skill>();

            this.CreateDiscriminatedUnionMap<ISkillNameValidationError>();
            CreateMap<SkillValidationErrors, SkillValidationErrorsModel>();

            CreateMap<UpdateSkillModel, Skill>();
        }
    }
}
