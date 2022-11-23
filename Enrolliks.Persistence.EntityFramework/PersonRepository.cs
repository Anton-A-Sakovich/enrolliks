using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Enrolliks.Persistence.People;
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

        public async Task<ICreatePersonResult> CreateAsync(Person person)
        {
            _context.Add(person);
            await _context.SaveChangesAsync();
            return new ICreatePersonResult.Success(person);
        }

        public async Task<IDeletePersonResult> DeleteAsync(string name)
        {
            var personExistsResult = await ExistsAsync(name);
            switch (personExistsResult)
            {
                case IExistsPersonResult.Success(bool exists):
                    if (exists)
                    {
                        var person = new Person(name);
                        _context.Remove(person);

                        await _context.SaveChangesAsync();

                        return new IDeletePersonResult.Success();
                    }
                    else
                    {
                        return new IDeletePersonResult.NotFound();
                    }
                case IExistsPersonResult.RepositoryFailure(Exception exception):
                    return new IDeletePersonResult.RepositoryFailure(exception);
                default:
                    throw new ApplicationException("The switch cases were incomplete.");
            }
        }

        public async Task<IGetAllPeopleResult> GetAllAsync()
        {
            var people = await _context.People.ToListAsync();
            return new IGetAllPeopleResult.Success(people);
        }

        public async Task<IExistsPersonResult> ExistsAsync(string name)
        {
            bool exists = await _context.People.AnyAsync(person => person.Name == name);
            return new IExistsPersonResult.Success(exists);
        }

        public async Task<IUpdatePersonResult> UpdateAsync(string name, Person newPerson)
        {
            var existingPerson = await _context.People.FirstOrDefaultAsync(person => person.Name == name);
            if (existingPerson is null)
                return new IUpdatePersonResult.NotFound();

            _context.Entry(existingPerson).Property(person => person.Name).CurrentValue = newPerson.Name;

            _context.Update(existingPerson);
            await _context.SaveChangesAsync();
            return new IUpdatePersonResult.Success(existingPerson);
        }
    }
}
