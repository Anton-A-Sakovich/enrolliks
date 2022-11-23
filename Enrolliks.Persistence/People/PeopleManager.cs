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

        public async Task<IUpdatePersonResult> UpdateAsync(string name, Person newPerson)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (newPerson is null) throw new ArgumentNullException(nameof(newPerson));

            if (_validator.Validate(newPerson) is PersonValidationErrors validationErrors)
                return new IUpdatePersonResult.ValidationFailure(validationErrors);

            Exception originalException;
            try
            {
                return await _repository.UpdateAsync(name, newPerson);
            }
            catch (Exception exception)
            {
                originalException = exception;
            }

            (bool exists, bool existsFailed) = await TryCallExistsMethod(
                () => _repository.ExistsAsync(name),
                result => result is IExistsPersonResult.Success success ? success.Exists : null);

            if (existsFailed)
                return new IUpdatePersonResult.RepositoryFailure(originalException);

            if (!exists)
                return new IUpdatePersonResult.NotFound();

            (bool existsConflicting, bool existsConflictingFailed) = await TryCallExistsMethod(
                () => _repository.ExistsAsync(newPerson.Name),
                result => result is IExistsPersonResult.Success success ? success.Exists : null);

            if (existsConflictingFailed)
                return new IUpdatePersonResult.RepositoryFailure(originalException);

            if (existsConflicting)
                return new IUpdatePersonResult.Conflict();

            return new IUpdatePersonResult.RepositoryFailure(originalException);
        }

        private static async Task<(bool Exists, bool Failed)> TryCallExistsMethod<T>(Func<Task<T>> existsMethod, Func<T, bool?> converter)
        {
            bool existsFailed = true;
            bool exists = false;
            try
            {
                var existsResult = await existsMethod();
                if (converter(existsResult) is bool existsValue)
                {
                    existsFailed = false;
                    exists = existsValue;
                }
            }
            catch
            {
                // Use the original exception instead.
            }

            return (exists, existsFailed);
        }
    }
}
