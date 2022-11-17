using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enrolliks.Persistence.People
{
    internal class PeopleManager : IPeopleManager
    {
        private const int _minNameLength = 3;
        private const int _maxNameLength = 128;

        private static readonly Regex _disallowedLetterRegex = new(@"[^A-Za-z\ ]", RegexOptions.Compiled);

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

            if (Validate(person) is PersonValidationErrors validationErrors)
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

        private static PersonValidationErrors? Validate(Person person)
        {
            IPersonNameValidationError? nameError = person.Name switch
            {
                var s when string.IsNullOrWhiteSpace(s) => new IPersonNameValidationError.Empty(),

                string s when s.Length < _minNameLength => new IPersonNameValidationError.TooShort(_minNameLength),
                string s when s.Length > _maxNameLength => new IPersonNameValidationError.TooLong(_maxNameLength),

                string s when char.IsWhiteSpace(s, 0) => new IPersonNameValidationError.StartsWithSpace(),

                string s when _disallowedLetterRegex.Match(s) is Match { Success: true } match
                    => new IPersonNameValidationError.DisallowedLetter(s.Substring(match.Index, match.Length)),

                _ => null,
            };

            if (nameError is not null)
                return new PersonValidationErrors { Name = nameError };

            return null;
        }
    }
}
