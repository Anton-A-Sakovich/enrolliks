using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Enrolliks.Persistence.Skills;
using Moq;
using NUnit.Framework;

namespace Enrolliks.Persistence.Tests.Skills
{
    internal static class PeopleManagerTestsExtensions
    {
        public static MockBuilder<ISkillValidator> ValidatesSkill(this MockBuilder<ISkillValidator> builder, Skill skill)
        {
            builder.Setup(validator => validator.Setup(v => v.Validate(skill)).Returns((SkillValidationErrors?)null));
            return builder;
        }

        public static MockBuilder<ISkillsRepository> Returns<T>(this MockBuilder<ISkillsRepository> builder, Expression<Func<ISkillsRepository, Task<T>>> method, T result)
        {
            builder.Setup(repository => repository.Setup(method).ReturnsAsync(result));
            return builder;
        }

        public static MockBuilder<ISkillsRepository> Throws<TReturn, TException>(this MockBuilder<ISkillsRepository> builder, Expression<Func<ISkillsRepository, Task<TReturn>>> method, TException exception)
            where TException : Exception
        {
            builder.Setup(repository => repository.Setup(method).ThrowsAsync(exception));
            return builder;
        }
    }

    [TestFixture]
    public class SkillsManagerTests
    {
        [TestFixture]
        public class CreateTests
        {
            [Test]
            public void ThrowsForNullSkill()
            {
                var builder = new SkillManagerTestBuilder();
                builder.AssertionsBuilder.Assert(async manager => await manager.CreateAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [Test]
            public void ReturnsRepositoryResult()
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");
                var createdSkill = new Skill(Id: "2", Name: ".NET");

                var repositoryResults = new ICreateSkillResult[]
                {
                    new ICreateSkillResult.Conflict(),
                    new ICreateSkillResult.RepositoryFailure(new Exception()),
                    new ICreateSkillResult.Created(createdSkill),
                };

                foreach (var repositoryResult in repositoryResults)
                {
                    var builder = new SkillManagerTestBuilder();
                    builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                    builder.RepositoryBuilder.Returns(repository => repository.CreateAsync(skillToCreate), repositoryResult);
                    builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.SameAs(repositoryResult));
                    builder.Test();
                }
            }

            [Test]
            public void DetectsConflictManually()
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");

                var builder = new SkillManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Throws(repository => repository.CreateAsync(skillToCreate), new Exception());
                builder.RepositoryBuilder.Returns(repository => repository.ExistsWithNameAsync(skillToCreate.Name), new ISkillWithNameExistsResult.Success(Exists: true));
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.EqualTo(new ICreateSkillResult.Conflict()));
                builder.Test();
            }

            [Test]
            public void ReturnsOriginalRepositoryExceptionWhenExistsReturnsFalse()
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");
                var originalException = new Exception();

                var builder = new SkillManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Throws(repository => repository.CreateAsync(skillToCreate), originalException);
                builder.RepositoryBuilder.Returns(repository => repository.ExistsWithNameAsync(skillToCreate.Name), new ISkillWithNameExistsResult.Success(Exists: false));
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.EqualTo(new ICreateSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }

            [Test]
            public void ReturnsOriginalRepositoryExceptionWhenExistsReturnsFailure()
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");
                var originalException = new Exception();

                var builder = new SkillManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Throws(repository => repository.CreateAsync(skillToCreate), originalException);
                builder.RepositoryBuilder.Returns(repository => repository.ExistsWithNameAsync(skillToCreate.Name), new ISkillWithNameExistsResult.RepositoryFailure(new Exception()));
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.EqualTo(new ICreateSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }

            [Test]
            public void ReturnsOriginalRepositoryExceptionWhenExistsThrows()
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");
                var originalException = new Exception();

                var builder = new SkillManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Throws(repository => repository.CreateAsync(skillToCreate), originalException);
                builder.RepositoryBuilder.Throws(repository => repository.ExistsWithNameAsync(skillToCreate.Name), new Exception());
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.EqualTo(new ICreateSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }
        }
    }
}
