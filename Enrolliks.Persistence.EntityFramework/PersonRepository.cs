using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Enrolliks.Persistence.EntityFramework
{
    internal class PeopleRepository : IPeopleRepository
    {
        private readonly EnrolliksContext _context;

        public PeopleRepository(EnrolliksContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Person person)
        {
            _context.Add(person);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string name)
        {
            bool personExists = await ExistsAsync(name);
            if (personExists)
            {
                var person = new Person(name);
                _context.Remove(person);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IList<Person>> GetAllAsync()
        {
            var people = await _context.People.ToListAsync();
            return people;
        }

        public async Task<bool> ExistsAsync(string name)
        {
            bool exists = await _context.People.AnyAsync(person => person.Name == name);
            return exists;
        }
    }
}
