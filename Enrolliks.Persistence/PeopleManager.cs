using System;
using System.Threading.Tasks;

namespace Enrolliks.Persistence
{
    internal class PeopleManager : IPeopleManager
    {
        private const int _minNameLength = 3;
        private const int _maxNameLength = 128;

        private readonly IPeopleRepository _repository;

        public PeopleManager(IPeopleRepository repository)
        {
            _repository = repository;
        }

        public async Task<ICreatePersonResult> CreateAsync(Person person)
        {
            if (person is null) throw new ArgumentNullException(nameof(person));

            if (Validate(person) is PersonValidationErrors validationErrors)
                return new ICreatePersonResult.ValidationFailure(validationErrors);

            Exception createException;
            try
            {
                return await _repository.CreateAsync(person);
            }
            catch (Exception exception)
            {
                createException = exception;
            }

            try
            {
                var existsResult = await _repository.ExistsAsync(person.Name);
                if (existsResult is IExistsPersonResult.Success(bool exists) && exists)
                    return new ICreatePersonResult.Conflict();
            }
            catch
            {
                // Ignore the exists exception and return the original create exception if not able to reliably detect a conflict.
            }

            return new ICreatePersonResult.RepositoryFailure(createException);
        }

        public async Task<IDeletePersonResult> DeleteAsync(string name)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));

            Exception deleteException;
            try
            {
                return await _repository.DeleteAsync(name);
            }
            catch (Exception exception)
            {
                deleteException = exception;
            }

            try
            {
                var existsResult = await _repository.ExistsAsync(name);
                if (existsResult is IExistsPersonResult.Success(bool exists) && !exists)
                    return new IDeletePersonResult.NotFound();
            }
            catch
            {
                // Ignore the exists exception and return the original delete exception if not able to reliably detect a missing person.
            }

            return new IDeletePersonResult.RepositoryFailure(deleteException);
        }

        public async Task<IExistsPersonResult> ExistsAsync(string name)
        {
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
            if (Validate(person) is PersonValidationErrors validationErrors)
                return new IUpdatePersonResult.ValidationFailure(validationErrors);

            try
            {
                return await _repository.UpdateAsync(person);
            }
            catch (Exception exception)
            {
                return new IUpdatePersonResult.RepositoryFailure(exception);
            }
        }

        private static PersonValidationErrors? Validate(Person person)
        {
            INameValidationError? nameError = person.Name switch
            {
                null => new INameValidationError.Empty(),
                string s when string.IsNullOrWhiteSpace(s) => new INameValidationError.Empty(),
                string s when s.Length < _minNameLength => new INameValidationError.TooShort(_minNameLength),
                string s when s.Length > _maxNameLength => new INameValidationError.TooLong(_maxNameLength),
                _ => null,
            };

            if (nameError is not null)
                return new PersonValidationErrors { Name = nameError };

            return null;
        }
    }
}
