﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Enrolliks.Persistence.People;
using Moq;
using NUnit.Framework;

namespace Enrolliks.Persistence.Tests.People
{
    internal static class PeopleManagerTestsExtensions
    {
        public static MockBuilder<IPersonValidator> ValidatesPerson(this MockBuilder<IPersonValidator> builder, Person person)
        {
            builder.Setup(validator => validator.Setup(v => v.Validate(person)).Returns((PersonValidationErrors?)null));
            return builder;
        }

        public static MockBuilder<IPeopleRepository> Returns<T>(this MockBuilder<IPeopleRepository> builder, Expression<Func<IPeopleRepository, Task<T>>> method, T result)
        {
            builder.Setup(repository => repository.Setup(method).ReturnsAsync(result));
            return builder;
        }

        public static MockBuilder<IPeopleRepository> Throws<TReturn, TException>(this MockBuilder<IPeopleRepository> builder, Expression<Func<IPeopleRepository, Task<TReturn>>> method, TException exception)
            where TException : Exception
        {
            builder.Setup(repository => repository.Setup(method).ThrowsAsync(exception));
            return builder;
        }
    }

    [TestFixture]
    public class PeopleManagerTests
    {
        [TestFixture]
        public class CreateTests
        {
            [Test]
            public void ThrowsForNullPerson()
            {
                var builder = new PeopleManagerTestBuilder();
                builder.AssertionsBuilder.Assert(async manager => await manager.CreateAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [TestCaseSource(nameof(GetRepositoryResults))]
            public void ReturnsRepositoryResult(Person person, ICreatePersonResult repositoryResult)
            {
                var builder = new PeopleManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesPerson(person);
                builder.RepositoryBuilder.Returns(repository => repository.CreateAsync(person), repositoryResult);
                builder.AssertionsBuilder.Assert(async manager => await manager.CreateAsync(person), Is.SameAs(repositoryResult));
                builder.Test();
            }

            private static IEnumerable<object[]> GetRepositoryResults()
            {
                var person = new Person(Name: "Joe");
                var repositoryResults = new ICreatePersonResult[]
                {
                    new ICreatePersonResult.Conflict(),
                    new ICreatePersonResult.RepositoryFailure(new Exception()),
                    new ICreatePersonResult.Success(person),
                };

                return repositoryResults.Select(result => new object[] { person, result });
            }

            [Test]
            public void DetectsConflictManually()
            {
                var person = new Person(Name: "Already exists");
                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(person);

                builder.RepositoryBuilder
                    .Throws(repository => repository.CreateAsync(person), new Exception())
                    .Returns(repository => repository.ExistsAsync(person.Name), new IExistsPersonResult.Success(Exists: true));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.CreateAsync(person), Is.EqualTo(new ICreatePersonResult.Conflict()));

                builder.Test();
            }

            [TestCaseSource(nameof(GetExistsSetups))]
            public void ReturnsOriginalRepositoryException(Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsSetup)
            {
                var person = new Person(Name: "Joe");
                var originalException = new Exception();

                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(person);

                builder.RepositoryBuilder
                    .Throws(repository => repository.CreateAsync(person), originalException)
                    .Setup(mock => existsSetup(mock.Setup(repository => repository.ExistsAsync(person.Name))));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.CreateAsync(person), Is.EqualTo(new ICreatePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }

            private static IEnumerable<object[]> GetExistsSetups()
            {
                Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsSetup;

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new IExistsPersonResult.Success(Exists: false));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new IExistsPersonResult.RepositoryFailure(new Exception()));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { existsSetup };
            }
        }

        [TestFixture]
        public class DeleteTests
        {
            [Test]
            public void ThrowsForNullName()
            {
                var builder = new PeopleManagerTestBuilder();
                builder.AssertionsBuilder.Assert(async manager => await manager.DeleteAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [TestCaseSource(nameof(GetRepositoryResults))]
            public void ReturnsRepositoryResult(string name, IDeletePersonResult repositoryResult)
            {
                var builder = new PeopleManagerTestBuilder();
                builder.RepositoryBuilder.Returns(repository => repository.DeleteAsync(name), repositoryResult);
                builder.AssertionsBuilder.Assert(async manager => await manager.DeleteAsync(name), Is.SameAs(repositoryResult));
                builder.Test();
            }

            protected static IEnumerable<object[]> GetRepositoryResults()
            {
                string name = "Joe";
                var repositoryResults = new IDeletePersonResult[]
                {
                    new IDeletePersonResult.NotFound(),
                    new IDeletePersonResult.RepositoryFailure(new Exception()),
                    new IDeletePersonResult.Success(),
                };

                return repositoryResults.Select(result => new object[] { name, result });
            }

            [Test]
            public void DetectsMissingPersonManually()
            {
                string name = "Not found";

                var builder = new PeopleManagerTestBuilder();

                builder.RepositoryBuilder
                    .Throws(repository => repository.DeleteAsync(name), new Exception())
                    .Returns(repository => repository.ExistsAsync(name), new IExistsPersonResult.Success(Exists: false));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.DeleteAsync(name), Is.EqualTo(new IDeletePersonResult.NotFound()));

                builder.Test();
            }

            [TestCaseSource(nameof(GetExistsSetups))]
            public void ReturnsOriginalRepositoryException(Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsSetup)
            {
                string name = "Exception thrower";
                var originalException = new Exception();

                var builder = new PeopleManagerTestBuilder();

                builder.RepositoryBuilder
                    .Throws(repository => repository.DeleteAsync(name), originalException)
                    .Setup(mock => existsSetup(mock.Setup(repository => repository.ExistsAsync(name))));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.DeleteAsync(name), Is.EqualTo(new IDeletePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }

            protected static IEnumerable<object[]> GetExistsSetups()
            {
                Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsSetup;

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new IExistsPersonResult.Success(Exists: true));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new IExistsPersonResult.RepositoryFailure(new Exception()));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { existsSetup };
            }
        }

        [TestFixture]
        public class UpdateTests
        {
            [Test]
            public void ThrowsForNullName()
            {
                var builder = new PeopleManagerTestBuilder();
                builder.AssertionsBuilder.Assert(async manager => await manager.UpdateAsync(name: null!, newPerson: new Person(Name: "Updated Joe")), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [Test]
            public void ThrowsForNullPerson()
            {
                var builder = new PeopleManagerTestBuilder();
                builder.AssertionsBuilder.Assert(async manager => await manager.UpdateAsync(name: "Joe", newPerson: null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [TestCaseSource(nameof(GetRepositoryResults))]
            public void ReturnsRepositoryResult(string name, Person newPerson, IUpdatePersonResult repositoryResult)
            {
                var builder = new PeopleManagerTestBuilder();
                builder.ValidatorBuilder.ValidatesPerson(newPerson);
                builder.RepositoryBuilder.Returns(repository => repository.UpdateAsync(name, newPerson), repositoryResult);
                builder.AssertionsBuilder.Assert(async manager => await manager.UpdateAsync(name, newPerson), Is.SameAs(repositoryResult));
                builder.Test();
            }

            protected static IEnumerable<object[]> GetRepositoryResults()
            {
                string name = "Joe";
                var newPerson = new Person("Updated Joe");

                var repositoryResults = new IUpdatePersonResult[]
                {
                    new IUpdatePersonResult.Conflict(),
                    new IUpdatePersonResult.NotFound(),
                    new IUpdatePersonResult.RepositoryFailure(new Exception()),
                    new IUpdatePersonResult.Success(newPerson),
                };

                return repositoryResults.Select(result => new object[] { name, newPerson, result });
            }

            [Test]
            public void DetectsMissingPersonManually()
            {
                string name = "Joe";
                var newPerson = new Person(Name: "Updated Joe");

                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(newPerson);

                builder.RepositoryBuilder
                    .Throws(repository => repository.UpdateAsync(name, newPerson), new Exception())
                    .Returns(repository => repository.ExistsAsync(name), new IExistsPersonResult.Success(Exists: false));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.UpdateAsync(name, newPerson), Is.EqualTo(new IUpdatePersonResult.NotFound()));

                builder.Test();
            }

            [Test]
            public void DetectsConflictingPersonManually()
            {
                string name = "Joe";
                var newPerson = new Person(Name: "Updated Joe");

                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(newPerson);

                builder.RepositoryBuilder
                    .Throws(repository => repository.UpdateAsync(name, newPerson), new Exception())
                    .Returns(repository => repository.ExistsAsync(name), new IExistsPersonResult.Success(Exists: true))
                    .Returns(repository => repository.ExistsAsync(newPerson.Name), new IExistsPersonResult.Success(Exists: true));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.UpdateAsync(name, newPerson), Is.EqualTo(new IUpdatePersonResult.Conflict()));

                builder.Test();
            }

            [TestCaseSource(nameof(GetOldExistsSetups))]
            public void ReturnsOriginalRepositoryExceptionWhenOldExistsFails(Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsSetup)
            {
                string name = "Joe";
                var newPerson = new Person(Name: "Updated Joe");
                var originalException = new Exception();

                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(newPerson);

                builder.RepositoryBuilder
                    .Throws(repository => repository.UpdateAsync(name, newPerson), originalException)
                    .Setup(mock => existsSetup(mock.Setup(repository => repository.ExistsAsync(name))));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.UpdateAsync(name, newPerson), Is.EqualTo(new IUpdatePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }

            protected static IEnumerable<object[]> GetOldExistsSetups()
            {
                Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsSetup;

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new IExistsPersonResult.RepositoryFailure(new Exception()));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { existsSetup };
            }

            [TestCaseSource(nameof(GetNewExistsSetups))]
            public void ReturnsOriginalRepositoryExceptionWhenNewExistsFails(Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsSetup)
            {
                string name = "Joe";
                var newPerson = new Person(Name: "Updated Joe");
                var originalException = new Exception();

                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(newPerson);

                builder.RepositoryBuilder
                    .Throws(repository => repository.UpdateAsync(name, newPerson), originalException)
                    .Returns(repository => repository.ExistsAsync(name), new IExistsPersonResult.Success(Exists: true))
                    .Setup(mock => existsSetup(mock.Setup(repository => repository.ExistsAsync(newPerson.Name))));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.UpdateAsync(name, newPerson), Is.EqualTo(new IUpdatePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }

            protected static IEnumerable<object[]> GetNewExistsSetups()
            {
                Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsSetup;

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new IExistsPersonResult.Success(Exists: false));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ReturnsAsync(new IExistsPersonResult.RepositoryFailure(new Exception()));
                yield return new object[] { existsSetup };

                existsSetup = existsMethod => existsMethod.ThrowsAsync(new Exception());
                yield return new object[] { existsSetup };
            }
        }

        [TestFixture]
        public class GetAllTests
        {
            [Test]
            public void ReturnsRepositoryResult()
            {
                var repositoryResults = new IGetAllPeopleResult[]
                {
                    new IGetAllPeopleResult.RepositoryFailure(new Exception()),
                    new IGetAllPeopleResult.Success(new List<Person>()),
                };

                Assert.Multiple(() =>
                {
                    foreach (var repositoryResult in repositoryResults)
                    {
                        var builder = new PeopleManagerTestBuilder();
                        builder.RepositoryBuilder.Returns(repository => repository.GetAllAsync(), repositoryResult);
                        builder.AssertionsBuilder.Assert(async manager => await manager.GetAllAsync(), Is.SameAs(repositoryResult));
                        builder.Test();
                    }
                });
            }

            [Test]
            public void ReturnsRepositoryFailure()
            {
                var exception = new Exception();

                var builder = new PeopleManagerTestBuilder();
                builder.RepositoryBuilder.Throws(repository => repository.GetAllAsync(), exception);
                builder.AssertionsBuilder.Assert(async manager => await manager.GetAllAsync(), Is.EqualTo(new IGetAllPeopleResult.RepositoryFailure(exception)));
                builder.Test();
            }
        }

        [TestFixture]
        public class ExistsTests
        {
            [Test]
            public void ThrowsForNullName()
            {
                var builder = new PeopleManagerTestBuilder();
                builder.AssertionsBuilder.Assert(async manager => await manager.ExistsAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [Test]
            public void ReturnsRepositoryResult()
            {
                string name = "Joe";
                var repositoryResults = new IExistsPersonResult[]
                {
                    new IExistsPersonResult.RepositoryFailure(new Exception()),
                    new IExistsPersonResult.Success(Exists: true),
                    new IExistsPersonResult.Success(Exists: false),
                };

                Assert.Multiple(() =>
                {
                    foreach (var repositoryResult in repositoryResults)
                    {
                        var builder = new PeopleManagerTestBuilder();
                        builder.RepositoryBuilder.Returns(repository => repository.ExistsAsync(name), repositoryResult);
                        builder.AssertionsBuilder.Assert(async manager => await manager.ExistsAsync(name), Is.SameAs(repositoryResult));
                        builder.Test();
                    }
                });
            }

            [Test]
            public void ReturnsRepositoryFailure()
            {
                string name = "Joe";
                var exception = new Exception();

                var builder = new PeopleManagerTestBuilder();
                builder.RepositoryBuilder.Throws(repository => repository.ExistsAsync(name), exception);
                builder.AssertionsBuilder.Assert(async manager => await manager.ExistsAsync(name), Is.EqualTo(new IExistsPersonResult.RepositoryFailure(exception)));
                builder.Test();
            }
        }
    }
}
