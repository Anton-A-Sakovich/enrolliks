using System;
using System.Threading.Tasks;
using AutoMapper;
using Enrolliks.Persistence.People;
using Microsoft.EntityFrameworkCore;

namespace Enrolliks.Persistence.EntityFramework.People
{
    internal class PeopleRepository : IPeopleRepository
    {
        private readonly EnrolliksContext _context;
        private readonly IMapper _mapper;

        public PeopleRepository(EnrolliksContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ICreatePersonResult> CreateAsync(Person personToCreate)
        {
            if (personToCreate is null) throw new ArgumentNullException(nameof(personToCreate));

            var entity = _mapper.Map<Person, PersonEntity>(personToCreate);
            _context.People.Add(entity);
            await _context.SaveChangesAsync();

            var createdPerson = _mapper.Map<PersonEntity, Person>(entity);
            return new ICreatePersonResult.Success(createdPerson);
        }

        public async Task<IDeletePersonResult> DeleteAsync(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            var personExistsResult = await ExistsAsync(name);
            switch (personExistsResult)
            {
                case IExistsPersonResult.Success(bool exists):
                    if (exists)
                    {
                        var entity = new PersonEntity { Name = name };
                        _context.People.Remove(entity);
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
                    throw new SwitchFailureException();
            }
        }

        public async Task<IGetAllPeopleResult> GetAllAsync()
        {
            var people = await _mapper.ProjectTo<Person>(_context.People.AsNoTracking()).ToListAsync();
            return new IGetAllPeopleResult.Success(people);
        }

        public async Task<IExistsPersonResult> ExistsAsync(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            bool exists = await _context.People.AnyAsync(person => person.Name == name);
            return new IExistsPersonResult.Success(exists);
        }

        public async Task<IUpdatePersonResult> UpdateAsync(string name, Person newPerson)
        {
            var entity = await _context.People.FirstOrDefaultAsync(person => person.Name == name);
            if (entity is null)
                return new IUpdatePersonResult.NotFound();

            _ = _mapper.Map(newPerson, entity);
            await _context.SaveChangesAsync();

            var updatedPerson = _mapper.Map<PersonEntity, Person>(entity);
            return new IUpdatePersonResult.Success(updatedPerson);
        }
    }
}
