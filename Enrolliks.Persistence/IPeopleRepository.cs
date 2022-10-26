using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrolliks.Persistence
{
    public interface IPeopleRepository
    {
        Task<IList<Person>> GetAllAsync();

        Task CreateAsync(Person person);

        Task DeleteAsync(string name);
    }
}
