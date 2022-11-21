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
                ReturnsOriginalRepositoryExceptionWhen(repositoryExistsMethod => repositoryExistsMethod.ReturnsAsync(new ISkillWithNameExistsResult.Success(Exists: false)));
            }

            [Test]
            public void ReturnsOriginalRepositoryExceptionWhenExistsReturnsFailure()
            {
                ReturnsOriginalRepositoryExceptionWhen(repositoryExistsMethod => repositoryExistsMethod.ReturnsAsync(new ISkillWithNameExistsResult.RepositoryFailure(new Exception())));
            }

            [Test]
            public void ReturnsOriginalRepositoryExceptionWhenExistsThrows()
            {
                ReturnsOriginalRepositoryExceptionWhen(repositoryExistsMethod => repositoryExistsMethod.ThrowsAsync(new Exception()));
            }

            private static void ReturnsOriginalRepositoryExceptionWhen(Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillWithNameExistsResult>>> existsSetupAction)
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");
                var originalException = new Exception();

                var builder = new SkillManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Throws(repository => repository.CreateAsync(skillToCreate), originalException);
                builder.RepositoryBuilder.Setup(repository => existsSetupAction(repository.Setup(r => r.ExistsWithNameAsync(skillToCreate.Name))));
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.EqualTo(new ICreateSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }
        }

        [TestFixture]
        public class DeleteTests
        {
            [Test]
            public void ThrowsForNullId()
            {
                var builder = new SkillManagerTestBuilder();
                builder.AssertionsBuilder.Assert(manager => manager.DeleteAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [Test]
            public void ReturnsRepositoryResult()
            {
                string id = "dot-net";
                var repositoryResults = new IDeleteSkillResult[]
                {
                    new IDeleteSkillResult.Deleted(),
                    new IDeleteSkillResult.NotFound(),
                    new IDeleteSkillResult.RepositoryFailure(new Exception()),
                };

                foreach (var repositoryResult in repositoryResults)
                {
                    var builder = new SkillManagerTestBuilder();
                    builder.RepositoryBuilder.Returns(repository => repository.DeleteAsync(id), repositoryResult);
                    builder.AssertionsBuilder.Assert(manager => manager.DeleteAsync(id), Is.SameAs(repositoryResult));
                    builder.Test();
                }
            }

            [Test]
            public void DetectsMissingSkillManually()
            {
                string id = "dot-net";

                var builder = new SkillManagerTestBuilder();
                builder.RepositoryBuilder.Throws(repository => repository.DeleteAsync(id), new Exception());
                builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(id), new ISkillExistsResult.Success(Exists: false));
                builder.AssertionsBuilder.Assert(manager => manager.DeleteAsync(id), Is.EqualTo(new IDeleteSkillResult.NotFound()));
                builder.Test();
            }

            [Test]
            public void ReturnsOriginalExceptionWhenExistsReturnsTrue()
            {
                ReturnsOriginalExceptionWhen(repositoryExistsMethod => repositoryExistsMethod.ReturnsAsync(new ISkillExistsResult.Success(Exists: true)));
            }

            [Test]
            public void ReturnsOriginalExceptionWhenExistsReturnsFailure()
            {
                ReturnsOriginalExceptionWhen(repositoryExistsMethod => repositoryExistsMethod.ReturnsAsync(new ISkillExistsResult.RepositoryFailure(new Exception())));
            }

            [Test]
            public void ReturnsOriginalExceptionWhenExistsThrows()
            {
                ReturnsOriginalExceptionWhen(repositoryExistsMethod => repositoryExistsMethod.ThrowsAsync(new Exception()));
            }

            private static void ReturnsOriginalExceptionWhen(Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillExistsResult>>> existsSetupAction)
            {
                string id = "dot-net";
                var originalException = new Exception();

                var builder = new SkillManagerTestBuilder();
                builder.RepositoryBuilder.Throws(repository => repository.DeleteAsync(id), originalException);
                builder.RepositoryBuilder.Setup(repository => existsSetupAction(repository.Setup(r => r.ExistsAsync(id))));
                builder.AssertionsBuilder.Assert(manager => manager.DeleteAsync(id), Is.EqualTo(new IDeleteSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }
        }
    }
}
