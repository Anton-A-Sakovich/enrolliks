using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrolliks.Persistence
{
    public interface IPeopleRepository
    {
        Task CreateAsync(Person person);

        Task DeleteAsync(string name);

        Task<bool> ExistsAsync(string name);

        Task<IList<Person>> GetAllAsync();
    }
}
