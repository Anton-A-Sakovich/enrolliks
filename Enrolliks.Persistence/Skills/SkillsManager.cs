using System;
using System.Threading.Tasks;

namespace Enrolliks.Persistence.Skills
{
    internal class SkillsManager : ISkillsManager
    {
        private readonly ISkillsRepository _repository;
        private readonly ISkillValidator _validator;

        public SkillsManager(ISkillsRepository repository, ISkillValidator validator)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        public async Task<ICreateSkillResult> CreateAsync(Skill skill)
        {
            if (skill is null) throw new ArgumentNullException(nameof(skill));

            if (_validator.Validate(skill) is SkillValidationErrors errors)
                return new ICreateSkillResult.ValidationFailure(errors);

            Exception originalException;
            try
            {
                return await _repository.CreateAsync(skill);
            }
            catch (Exception exception)
            {
                originalException = exception;
            }

            try
            {
                var existsResult = await _repository.ExistsWithNameAsync(skill.Name);
                if (existsResult is ISkillWithNameExistsResult.Success { Exists: true })
                    return new ICreateSkillResult.Conflict();
            }
            catch
            {
                // Use the original exception instead.
            }

            return new ICreateSkillResult.RepositoryFailure(originalException);
        }

        public async Task<IDeleteSkillResult> DeleteAsync(string id)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));

            Exception originalException;
            try
            {
                return await _repository.DeleteAsync(id);
            }
            catch (Exception exception)
            {
                originalException = exception;
            }

            try
            {
                var existsResult = await _repository.ExistsAsync(id);
                if (existsResult is ISkillExistsResult.Success { Exists: false })
                    return new IDeleteSkillResult.NotFound();
            }
            catch
            {
                // Use the original exception instead.
            }

            return new IDeleteSkillResult.RepositoryFailure(originalException);
        }

        public async Task<IGetManySkillsResult> GetAllAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception exception)
            {
                return new IGetManySkillsResult.RepositoryFailure(exception);
            }
        }

        public async Task<IGetOneSkillResult> GetOneAsync(string id)
        {
            if (id is null) throw new ArgumentNullException(nameof(id));

            Exception originalException;
            try
            {
                return await _repository.GetOneAsync(id);
            }
            catch (Exception exception)
            {
                originalException = exception;
            }

            try
            {
                var existsResult = await _repository.ExistsAsync(id);
                if (existsResult is ISkillExistsResult.Success { Exists: false })
                    return new IGetOneSkillResult.NotFound();
            }
            catch
            {
                // Use the original exception instead.
            }

            return new IGetOneSkillResult.RepositoryFailure(originalException);
        }

        public async Task<IUpdateSkillResult> UpdateAsync(Skill skill)
        {
            if (skill is null) throw new ArgumentNullException(nameof(skill));
            if (skill.Id is null) throw new ArgumentException("Cannot update a skill without ID", nameof(skill));

            if (_validator.Validate(skill) is SkillValidationErrors errors)
                return new IUpdateSkillResult.ValidationFailure(errors);

            Exception originalException;
            try
            {
                return await _repository.UpdateAsync(skill);
            }
            catch (Exception exception)
            {
                originalException = exception;
            }

            bool existsFailed = true;
            bool exists = false;
            try
            {
                var existsResult = await _repository.ExistsAsync(skill.Id);
                if (existsResult is ISkillExistsResult.Success success)
                {
                    existsFailed = false;
                    exists = success.Exists;
                }
            }
            catch
            {
                // Use the original exception instead.
            }

            if (existsFailed)
                return new IUpdateSkillResult.RepositoryFailure(originalException);

            if (!exists)
                return new IUpdateSkillResult.NotFound();

            bool existsWithNameFailed = true;
            bool existsWithName = false;
            try
            {
                var existsWithNameResult = await _repository.ExistsWithNameAsync(skill.Name);
                if (existsWithNameResult is ISkillWithNameExistsResult.Success success)
                {
                    existsWithNameFailed = false;
                    existsWithName = success.Exists;
                }
            }
            catch
            {
                // Use the original exception instead.
            }

            if (existsWithNameFailed)
                return new IUpdateSkillResult.RepositoryFailure(originalException);

            if (existsWithName)
                return new IUpdateSkillResult.Conflict();

            return new IUpdateSkillResult.RepositoryFailure(originalException);
        }
    }
}
