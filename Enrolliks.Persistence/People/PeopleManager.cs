using System;
using System.Threading.Tasks;

namespace Enrolliks.Persistence.People
{
    internal class PeopleManager : IPeopleManager
    {
        private readonly IPeopleRepository _repository;
        private readonly IPersonValidator _validator;

        public PeopleManager(IPeopleRepository repository, IPersonValidator validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<ICreatePersonResult> CreateAsync(Person person)
        {
            if (person is null) throw new ArgumentNullException(nameof(person));

            if (_validator.Validate(person) is PersonValidationErrors validationErrors)
                return new ICreatePersonResult.ValidationFailure(validationErrors);

            Exception originalException;
            try
            {
                return await _repository.CreateAsync(person);
            }
            catch (Exception exception)
            {
                originalException = exception;
            }

            try
            {
                var existsResult = await _repository.ExistsAsync(person.Name);
                if (existsResult is IExistsPersonResult.Success(bool exists) && exists)
                    return new ICreatePersonResult.Conflict();
            }
            catch
            {
                // Ignore the exists exception and return the original exception if not able to reliably detect a conflict.
            }

            return new ICreatePersonResult.RepositoryFailure(originalException);
        }

        public async Task<IDeletePersonResult> DeleteAsync(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            Exception originalException;
            try
            {
                return await _repository.DeleteAsync(name);
            }
            catch (Exception exception)
            {
                originalException = exception;
            }

            try
            {
                var existsResult = await _repository.ExistsAsync(name);
                if (existsResult is IExistsPersonResult.Success(bool exists) && !exists)
                    return new IDeletePersonResult.NotFound();
            }
            catch
            {
                // Ignore the exists exception and return the original exception if not able to reliably detect a missing person.
            }

            return new IDeletePersonResult.RepositoryFailure(originalException);
        }

        public async Task<IExistsPersonResult> ExistsAsync(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            try
            {
                return await _repository.ExistsAsync(name);
            }
            catch (Exception exception)
            {
                return new IExistsPersonResult.RepositoryFailure(exception);
            }
        }

        public async Task<IGetAllPeopleResult> GetAllAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception exception)
            {
                return new IGetAllPeopleResult.RepositoryFailure(exception);
            }
        }

        public async Task<IUpdatePersonResult> UpdateAsync(Person person)
        {
            if (person is null) throw new ArgumentNullException(nameof(person));

            if (_validator.Validate(person) is PersonValidationErrors validationErrors)
                return new IUpdatePersonResult.ValidationFailure(validationErrors);

            Exception originalException;
            try
            {
                return await _repository.UpdateAsync(person);
            }
            catch (Exception exception)
            {
                originalException = exception;
            }

            try
            {
                var existsResult = await _repository.ExistsAsync(person.Name);
                if (existsResult is IExistsPersonResult.Success(bool exists))
                    return exists ? new IUpdatePersonResult.Conflict() : new IUpdatePersonResult.NotFound();
            }
            catch
            {
                // Ignore the exists exception and return the original exception if not able to reliably detect a missing or conflicting person.
            }

            return new IUpdatePersonResult.RepositoryFailure(originalException);
        }
    }
}
