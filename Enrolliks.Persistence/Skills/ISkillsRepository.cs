using System.Threading.Tasks;

namespace Enrolliks.Persistence.Skills
{
    public interface ISkillsRepository
    {
        Task<ICreateSkillResult> CreateAsync(Skill skill);

        Task<ISkillWithNameExistsResult> ExistsWithNameAsync(string name);
    }
}
