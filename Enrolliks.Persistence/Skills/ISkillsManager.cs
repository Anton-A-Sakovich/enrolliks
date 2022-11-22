using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrolliks.Persistence.Skills
{
    public interface ISkillsManager
    {
        Task<ICreateSkillResult> CreateAsync(Skill skill);

        Task<IDeleteSkillResult> DeleteAsync(string id);

        Task<IGetManySkillsResult> GetAllAsync();

        Task<IGetOneSkillResult> GetOneAsync(string id);

        Task<IUpdateSkillResult> UpdateAsync(Skill skill);
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

    public interface IGetManySkillsResult
    {
        public record class Success(IList<Skill> Skills) : IGetManySkillsResult;

        public record class RepositoryFailure(Exception Exception) : IGetManySkillsResult;
    }

    public interface IGetOneSkillResult
    {
        public record class Success(Skill Skill) : IGetOneSkillResult;

        public record class NotFound() : IGetOneSkillResult;

        public record class RepositoryFailure(Exception Exception) : IGetOneSkillResult;
    }

    public interface IUpdateSkillResult
    {
        public record class Success(Skill UpdatedSkill) : IUpdateSkillResult;

        public record class ValidationFailure(SkillValidationErrors Errors) : IUpdateSkillResult;

        public record class NotFound() : IUpdateSkillResult;

        public record class Conflict() : IUpdateSkillResult;

        public record class RepositoryFailure(Exception Exception) : IUpdateSkillResult;
    }
}
