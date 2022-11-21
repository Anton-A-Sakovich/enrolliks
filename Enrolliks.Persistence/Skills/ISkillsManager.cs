using System;
using System.Threading.Tasks;

namespace Enrolliks.Persistence.Skills
{
    public interface ISkillsManager
    {
        Task<ICreateSkillResult> CreateAsync(Skill skill);
    }

    public interface ICreateSkillResult
    {
        public record class Created(Skill CreatedSkill) : ICreateSkillResult;

        public record class ValidationFailure(SkillValidationErrors Errors) : ICreateSkillResult;

        public record class Conflict() : ICreateSkillResult;

        public record class RepositoryFailure(Exception Exception) : ICreateSkillResult;
    }

    public interface ISkillWithNameExistsResult
    {
        public record class Success(bool Exists) : ISkillWithNameExistsResult;

        public record class RepositoryFailure(Exception exception) : ISkillWithNameExistsResult;
    }
}
