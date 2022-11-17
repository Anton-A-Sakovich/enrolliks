using System.Threading.Tasks;

namespace Enrolliks.Persistence.People
{
    public interface IPeopleRepository
    {
        Task<ICreatePersonResult> CreateAsync(Person person);

        Task<IDeletePersonResult> DeleteAsync(string name);

        Task<IExistsPersonResult> ExistsAsync(string name);

        Task<IGetAllPeopleResult> GetAllAsync();

        Task<IUpdatePersonResult> UpdateAsync(Person person);
    }
}
