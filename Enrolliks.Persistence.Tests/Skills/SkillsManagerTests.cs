﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Enrolliks.Persistence.Skills;
using Moq;
using NUnit.Framework;

namespace Enrolliks.Persistence.Tests.Skills
{
    internal static class SkillsManagerTestsExtensions
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
                var builder = new SkillsManagerTestBuilder();
                builder.AssertionsBuilder.Assert(async manager => await manager.CreateAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [Test]
            public void ReturnsValidationErrors()
            {
                var skill = new Skill(Id: "dot-net", Name: ".NET");
                var errors = new SkillValidationErrors { Name = new ISkillNameValidationError.Empty() };

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.Setup(mock => mock.Setup(validator => validator.Validate(skill)).Returns(errors));
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skill), Is.EqualTo(new ICreateSkillResult.ValidationFailure(errors)));
                builder.Test();
            }

            [TestCaseSource(nameof(GetRepositoryResults))]
            public void ReturnsRepositoryResult(Skill skillToCreate, ICreateSkillResult repositoryResult)
            {
                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Returns(repository => repository.CreateAsync(skillToCreate), repositoryResult);
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.SameAs(repositoryResult));
                builder.Test();
            }

            private static IEnumerable<object[]> GetRepositoryResults()
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");
                var createdSkill = new Skill(Id: "2", Name: ".NET");

                var repositoryResults = new ICreateSkillResult[]
                {
                    new ICreateSkillResult.Conflict(nameof(Skill.Id)),
                    new ICreateSkillResult.Conflict(nameof(Skill.Name)),
                    new ICreateSkillResult.RepositoryFailure(new Exception()),
                    new ICreateSkillResult.Created(createdSkill),
                };

                return repositoryResults.Select(result => new object[] { skillToCreate, result });
            }

            [Test]
            public void DetectsConflictingIdManually()
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Throws(repository => repository.CreateAsync(skillToCreate), new Exception());
                builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(skillToCreate.Id), new ISkillExistsResult.Success(Exists: true));
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.EqualTo(new ICreateSkillResult.Conflict(nameof(Skill.Id))));
                builder.Test();
            }

            [Test]
            public void DetectsConflictingNameManually()
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Throws(repository => repository.CreateAsync(skillToCreate), new Exception());
                builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(skillToCreate.Id), new ISkillExistsResult.Success(Exists: false));
                builder.RepositoryBuilder.Returns(repository => repository.ExistsWithNameAsync(skillToCreate.Name), new ISkillWithNameExistsResult.Success(Exists: true));
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.EqualTo(new ICreateSkillResult.Conflict(nameof(Skill.Name))));
                builder.Test();
            }

            [TestCaseSource(nameof(GetExistsMethodSetups))]
            public void ReturnsOriginalRepositoryExceptionWhenExistsFailed(Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillExistsResult>>> existsSetupAction)
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");
                var originalException = new Exception();

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Throws(repository => repository.CreateAsync(skillToCreate), originalException);
                builder.RepositoryBuilder.Setup(repository => existsSetupAction(repository.Setup(r => r.ExistsAsync(skillToCreate.Id))));
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.EqualTo(new ICreateSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }

            private static IEnumerable<object[]> GetExistsMethodSetups()
            {
                Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillExistsResult>>> existsSetupAction;

                existsSetupAction = existsMethod => existsMethod.ReturnsAsync(new ISkillExistsResult.RepositoryFailure(new Exception()));
                yield return new object[] { existsSetupAction };

                existsSetupAction = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { existsSetupAction };
            }

            [TestCaseSource(nameof(GetExistsWithNameMethodSetups))]
            public void ReturnsOriginalRepositoryExceptionWhenExistsWithNameFailed(Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillWithNameExistsResult>>> existsSetupAction)
            {
                var skillToCreate = new Skill(Id: "1", Name: ".NET");
                var originalException = new Exception();

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToCreate);
                builder.RepositoryBuilder.Throws(repository => repository.CreateAsync(skillToCreate), originalException);
                builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(skillToCreate.Id), new ISkillExistsResult.Success(Exists: false));
                builder.RepositoryBuilder.Setup(repository => existsSetupAction(repository.Setup(r => r.ExistsWithNameAsync(skillToCreate.Name))));
                builder.AssertionsBuilder.Assert(manager => manager.CreateAsync(skillToCreate), Is.EqualTo(new ICreateSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }

            private static IEnumerable<object[]> GetExistsWithNameMethodSetups()
            {
                Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillWithNameExistsResult>>> existsSetupAction;

                existsSetupAction = existsMethod => existsMethod.ReturnsAsync(new ISkillWithNameExistsResult.Success(Exists: false));
                yield return new object[] { existsSetupAction };

                existsSetupAction = existsMethod => existsMethod.ReturnsAsync(new ISkillWithNameExistsResult.RepositoryFailure(new Exception()));
                yield return new object[] { existsSetupAction };

                existsSetupAction = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { existsSetupAction };
            }
        }

        [TestFixture]
        public class DeleteTests
        {
            [Test]
            public void ThrowsForNullId()
            {
                var builder = new SkillsManagerTestBuilder();
                builder.AssertionsBuilder.Assert(manager => manager.DeleteAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [TestCaseSource(nameof(GetRepositoryResults))]
            public void ReturnsRepositoryResult(string id, IDeleteSkillResult repositoryResult)
            {
                var builder = new SkillsManagerTestBuilder();
                builder.RepositoryBuilder.Returns(repository => repository.DeleteAsync(id), repositoryResult);
                builder.AssertionsBuilder.Assert(manager => manager.DeleteAsync(id), Is.SameAs(repositoryResult));
                builder.Test();
            }

            private static IEnumerable<object[]> GetRepositoryResults()
            {
                string id = "dot-net";
                var repositoryResults = new IDeleteSkillResult[]
                {
                    new IDeleteSkillResult.Deleted(),
                    new IDeleteSkillResult.NotFound(),
                    new IDeleteSkillResult.RepositoryFailure(new Exception()),
                };

                return repositoryResults.Select(result => new object[] { id, result });
            }

            [Test]
            public void DetectsMissingSkillManually()
            {
                string id = "dot-net";

                var builder = new SkillsManagerTestBuilder();
                builder.RepositoryBuilder.Throws(repository => repository.DeleteAsync(id), new Exception());
                builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(id), new ISkillExistsResult.Success(Exists: false));
                builder.AssertionsBuilder.Assert(manager => manager.DeleteAsync(id), Is.EqualTo(new IDeleteSkillResult.NotFound()));
                builder.Test();
            }

            [TestCaseSource(nameof(GetExistsSetups))]
            public void ReturnsOriginalException(Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillExistsResult>>> existsSetupAction)
            {
                string id = "dot-net";
                var originalException = new Exception();

                var builder = new SkillsManagerTestBuilder();
                builder.RepositoryBuilder.Throws(repository => repository.DeleteAsync(id), originalException);
                builder.RepositoryBuilder.Setup(repository => existsSetupAction(repository.Setup(r => r.ExistsAsync(id))));
                builder.AssertionsBuilder.Assert(manager => manager.DeleteAsync(id), Is.EqualTo(new IDeleteSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }

            private static IEnumerable<object[]> GetExistsSetups()
            {
                Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillExistsResult>>> existsSetup;

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new ISkillExistsResult.Success(Exists: true));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new ISkillExistsResult.RepositoryFailure(new Exception()));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { existsSetup };
            }
        }

        [TestFixture]
        public class GetAllTests
        {
            [TestCaseSource(nameof(GetRepositoryResults))]
            public void ReturnsRepositoryResult(IGetManySkillsResult repositoryResult)
            {
                var builder = new SkillsManagerTestBuilder();
                builder.RepositoryBuilder.Returns(repository => repository.GetAllAsync(), repositoryResult);
                builder.AssertionsBuilder.Assert(manager => manager.GetAllAsync(), Is.SameAs(repositoryResult));
                builder.Test();
            }

            private static IEnumerable<object[]> GetRepositoryResults()
            {
                yield return new object[] { new IGetManySkillsResult.RepositoryFailure(new Exception()) };
                yield return new object[] { new IGetManySkillsResult.Success(new List<Skill>()) };
            }

            [Test]
            public void ReturnsException()
            {
                var exception = new Exception();
                var builder = new SkillsManagerTestBuilder();
                builder.RepositoryBuilder.Throws(repositopry => repositopry.GetAllAsync(), exception);
                builder.AssertionsBuilder.Assert(repository => repository.GetAllAsync(), Is.EqualTo(new IGetManySkillsResult.RepositoryFailure(exception)));
                builder.Test();
            }
        }

        [TestFixture]
        public class GetOneTests
        {
            [Test]
            public void ThrowsForNullId()
            {
                var builder = new SkillsManagerTestBuilder();
                builder.AssertionsBuilder.Assert(manager => manager.GetOneAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [TestCaseSource(nameof(GetRepositoryResults))]
            public void ReturnsRepositoryResult(string id, IGetOneSkillResult repositoryResult)
            {
                var builder = new SkillsManagerTestBuilder();
                builder.RepositoryBuilder.Returns(repository => repository.GetOneAsync(id), repositoryResult);
                builder.AssertionsBuilder.Assert(repository => repository.GetOneAsync(id), Is.SameAs(repositoryResult));
                builder.Test();
            }

            private static IEnumerable<object[]> GetRepositoryResults()
            {
                string id = "dot-net";

                yield return new object[] { id, new IGetOneSkillResult.NotFound() };
                yield return new object[] { id, new IGetOneSkillResult.RepositoryFailure(new Exception()) };
                yield return new object[] { id, new IGetOneSkillResult.Success(new Skill(Id: id, Name: ".NET")) };
            }

            [Test]
            public void DetectsMissingSkillManually()
            {
                string id = "dot-net";

                var builder = new SkillsManagerTestBuilder();
                builder.RepositoryBuilder.Throws(repository => repository.GetOneAsync(id), new Exception());
                builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(id), new ISkillExistsResult.Success(Exists: false));
                builder.AssertionsBuilder.Assert(manager => manager.GetOneAsync(id), Is.EqualTo(new IGetOneSkillResult.NotFound()));
                builder.Test();
            }

            [TestCaseSource(nameof(GetExistsSetups))]
            public void ReturnsOriginalException(Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillExistsResult>>> existsSetupAction)
            {
                string id = "dot-net";
                var originalException = new Exception();

                var builder = new SkillsManagerTestBuilder();
                builder.RepositoryBuilder.Throws(repository => repository.GetOneAsync(id), originalException);
                builder.RepositoryBuilder.Setup(repository => existsSetupAction(repository.Setup(r => r.ExistsAsync(id))));
                builder.AssertionsBuilder.Assert(manager => manager.GetOneAsync(id), Is.EqualTo(new IGetOneSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }

            private static IEnumerable<object[]> GetExistsSetups()
            {
                Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillExistsResult>>> existsSetup;

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new ISkillExistsResult.Success(Exists: true));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new ISkillExistsResult.RepositoryFailure(new Exception()));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { existsSetup };
            }
        }

        [TestFixture]
        public class UpdateTests
        {
            [Test]
            public void ThrowsForNullSkill()
            {
                var builder = new SkillsManagerTestBuilder();
                builder.AssertionsBuilder.Assert(manager => manager.UpdateAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [Test]
            public void ThrowsForSkillWithoutId()
            {
                var builder = new SkillsManagerTestBuilder();
                builder.AssertionsBuilder.Assert(manager => manager.UpdateAsync(new Skill(Id: null!, Name: ".NET")), Throws.TypeOf<ArgumentException>());
                builder.Test();
            }

            [Test]
            public void ReturnsValidationErrors()
            {
                var skill = new Skill(Id: "dot-net", Name: ".NET");
                var errors = new SkillValidationErrors { Name = new ISkillNameValidationError.Empty() };

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.Setup(mock => mock.Setup(validator => validator.Validate(skill)).Returns(errors));
                builder.AssertionsBuilder.Assert(manager => manager.UpdateAsync(skill), Is.EqualTo(new IUpdateSkillResult.ValidationFailure(errors)));
                builder.Test();
            }

            [TestCaseSource(nameof(GetRepositoryResults))]
            public void ReturnsRepositoryResult(Skill skillToUpdate, IUpdateSkillResult repositoryResult)
            {
                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToUpdate);
                builder.RepositoryBuilder.Returns(repository => repository.UpdateAsync(skillToUpdate), repositoryResult);
                builder.AssertionsBuilder.Assert(manager => manager.UpdateAsync(skillToUpdate), Is.SameAs(repositoryResult));
                builder.Test();
            }

            private static IEnumerable<object[]> GetRepositoryResults()
            {
                var skillToUpdate = new Skill(Id: "dot-net", Name: "NET");
                var updatedSkill = new Skill(Id: "dot-net", Name: ".NET");

                yield return new object[] { skillToUpdate, new IUpdateSkillResult.Conflict() };
                yield return new object[] { skillToUpdate, new IUpdateSkillResult.NotFound() };
                yield return new object[] { skillToUpdate, new IUpdateSkillResult.RepositoryFailure(new Exception()) };
                yield return new object[] { skillToUpdate, new IUpdateSkillResult.Success(updatedSkill) };
            }

            [Test]
            public void DetectsMissingSkillManually()
            {
                var skillToUpdate = new Skill(Id: "dot-net", Name: "NET");

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToUpdate);
                builder.RepositoryBuilder.Throws(repository => repository.UpdateAsync(skillToUpdate), new Exception());
                builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(skillToUpdate.Id), new ISkillExistsResult.Success(Exists: false));
                builder.AssertionsBuilder.Assert(manager => manager.UpdateAsync(skillToUpdate), Is.EqualTo(new IUpdateSkillResult.NotFound()));
                builder.Test();
            }

            [Test]
            public void DetectsConflictingSkillManually()
            {
                var skillToUpdate = new Skill(Id: "dot-net", Name: "NET");

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToUpdate);
                builder.RepositoryBuilder.Throws(repository => repository.UpdateAsync(skillToUpdate), new Exception());
                builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(skillToUpdate.Id), new ISkillExistsResult.Success(Exists: true));
                builder.RepositoryBuilder.Returns(repository => repository.ExistsWithNameAsync(skillToUpdate.Name), new ISkillWithNameExistsResult.Success(Exists: true));
                builder.AssertionsBuilder.Assert(manager => manager.UpdateAsync(skillToUpdate), Is.EqualTo(new IUpdateSkillResult.Conflict()));
                builder.Test();
            }

            [TestCaseSource(nameof(GetExistsResults))]
            public void ReturnsOriginalExceptionWhenExists(Skill skillToUpdate, Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillExistsResult>>> existsSetupAction)
            {
                var originalException = new Exception();

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToUpdate);
                builder.RepositoryBuilder.Throws(repository => repository.UpdateAsync(skillToUpdate), originalException);
                builder.RepositoryBuilder.Setup(mock => existsSetupAction(mock.Setup(repository => repository.ExistsAsync(skillToUpdate.Id))));
                builder.AssertionsBuilder.Assert(manager => manager.UpdateAsync(skillToUpdate), Is.EqualTo(new IUpdateSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }

            private static IEnumerable<object[]> GetExistsResults()
            {
                var skillToUpdate = new Skill(Id: "dot-net", Name: "NET");
                Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillExistsResult>>> existsSetup;

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new ISkillExistsResult.RepositoryFailure(new Exception()));
                yield return new object[] { skillToUpdate, existsSetup };

                existsSetup = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { skillToUpdate, existsSetup };
            }

            [TestCaseSource(nameof(GetExistsWithNameResults))]
            public void ReturnsOriginalExceptionWhenExistsWithName(Skill skillToUpdate, Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillWithNameExistsResult>>> existsSetupAction)
            {
                var originalException = new Exception();

                var builder = new SkillsManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesSkill(skillToUpdate);
                builder.RepositoryBuilder.Throws(repository => repository.UpdateAsync(skillToUpdate), originalException);
                builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(skillToUpdate.Id), new ISkillExistsResult.Success(Exists: true));
                builder.RepositoryBuilder.Setup(mock => existsSetupAction(mock.Setup(repository => repository.ExistsWithNameAsync(skillToUpdate.Name))));
                builder.AssertionsBuilder.Assert(manager => manager.UpdateAsync(skillToUpdate), Is.EqualTo(new IUpdateSkillResult.RepositoryFailure(originalException)));
                builder.Test();
            }

            private static IEnumerable<object[]> GetExistsWithNameResults()
            {
                var skillToUpdate = new Skill(Id: "dot-net", Name: "NET");
                Action<Moq.Language.Flow.ISetup<ISkillsRepository, Task<ISkillWithNameExistsResult>>> existsSetupAction;

                existsSetupAction = existsMethod => existsMethod.ReturnsAsync(new ISkillWithNameExistsResult.Success(Exists: false));
                yield return new object[] { skillToUpdate, existsSetupAction };

                existsSetupAction = existsMethod => existsMethod.ReturnsAsync(new ISkillWithNameExistsResult.RepositoryFailure(new Exception()));
                yield return new object[] { skillToUpdate, existsSetupAction };

                existsSetupAction = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { skillToUpdate, existsSetupAction };
            }
        }
    }
}
