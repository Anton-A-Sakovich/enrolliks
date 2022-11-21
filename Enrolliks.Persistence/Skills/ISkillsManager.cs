using System;
using System.Threading.Tasks;

namespace Enrolliks.Persistence.Skills
{
    public interface ISkillsManager
    {
        Task<ICreateSkillResult> CreateAsync(Skill skill);

        Task<IDeleteSkillResult> DeleteAsync(string id);
    }

    public interface ICreateSkillResult
    {
        public record class Created(Skill CreatedSkill) : ICreateSkillResult;

        public record class ValidationFailure(SkillValidationErrors Errors) : ICreateSkillResult;

        public record class Conflict() : ICreateSkillResult;

        public record class RepositoryFailure(Exception Exception) : ICreateSkillResult;
    }

    public interface IDeleteSkillResult
    {
        public record class Deleted() : IDeleteSkillResult;

        public record class NotFound() : IDeleteSkillResult;

        public record class RepositoryFailure(Exception Exception) : IDeleteSkillResult;
    }

    public interface ISkillExistsResult
    {
        public record class Success(bool Exists) : ISkillExistsResult;

        public record class RepositoryFailure(Exception Exception) : ISkillExistsResult;
    }

    public interface ISkillWithNameExistsResult
    {
        public record class Success(bool Exists) : ISkillWithNameExistsResult;

        public record class RepositoryFailure(Exception Exception) : ISkillWithNameExistsResult;
    }
}
