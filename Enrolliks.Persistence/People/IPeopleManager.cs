using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrolliks.Persistence.People
{
    public interface IPeopleManager
    {
        Task<ICreatePersonResult> CreateAsync(Person person);

        Task<IDeletePersonResult> DeleteAsync(string name);

        Task<IExistsPersonResult> ExistsAsync(string name);

        Task<IGetAllPeopleResult> GetAllAsync();

        Task<IUpdatePersonResult> UpdateAsync(Person person);
    }

    public interface ICreatePersonResult
    {
        public record class Success(Person CreatedPerson) : ICreatePersonResult;

        public record class ValidationFailure(PersonValidationErrors ValidationErrors) : ICreatePersonResult;

        public record class Conflict() : ICreatePersonResult;

        public record class RepositoryFailure(Exception Exception) : ICreatePersonResult;
    }

    public interface IDeletePersonResult
    {
        public record class Success() : IDeletePersonResult;

        public record class NotFound() : IDeletePersonResult;

        public record class RepositoryFailure(Exception Exception) : IDeletePersonResult;
    }

    public interface IUpdatePersonResult
    {
        public record class Success(Person UpdatedPerson) : IUpdatePersonResult;

        public record class ValidationFailure(PersonValidationErrors ValidationErrors) : IUpdatePersonResult;

        public record class Conflict() : IUpdatePersonResult;

        public record class NotFound() : IUpdatePersonResult;

        public record class RepositoryFailure(Exception Exception) : IUpdatePersonResult;
    }

    public interface IExistsPersonResult
    {
        public record class Success(bool Exists) : IExistsPersonResult;

        public record class RepositoryFailure(Exception Exception) : IExistsPersonResult;
    }

    public interface IGetAllPeopleResult
    {
        public record class Success(IList<Person> People) : IGetAllPeopleResult;

        public record class RepositoryFailure(Exception Exception) : IGetAllPeopleResult;
    }
}
