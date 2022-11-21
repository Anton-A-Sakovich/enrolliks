using System.Threading.Tasks;

namespace Enrolliks.Persistence.Skills
{
    public interface ISkillsRepository
    {
        Task<ICreateSkillResult> CreateAsync(Skill skill);

        Task<IDeleteSkillResult> DeleteAsync(string id);

        Task<ISkillExistsResult> ExistsAsync(string id);

        Task<ISkillWithNameExistsResult> ExistsWithNameAsync(string name);
    }
}
