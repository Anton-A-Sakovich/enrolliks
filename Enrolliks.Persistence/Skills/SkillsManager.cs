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
                // Just do nothing.
            }

            return new ICreateSkillResult.RepositoryFailure(originalException);
        }
    }
}
