using System;
using System.Collections.Generic;
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

            [Test]
            public void ReturnsRepositoryResult()
            {
                var person = new Person(Name: "Joe");
                var repositoryResults = new ICreatePersonResult[]
                {
                    new ICreatePersonResult.Conflict(),
                    new ICreatePersonResult.RepositoryFailure(new Exception()),
                    new ICreatePersonResult.Success(person),
                };

                Assert.Multiple(() =>
                {
                    foreach (var repositoryResult in repositoryResults)
                    {
                        var builder = new PeopleManagerTestBuilder();

                        builder.ValidatorBuilder.ValidatesPerson(person);
                        builder.RepositoryBuilder.Returns(repository => repository.CreateAsync(person), repositoryResult);
                        builder.AssertionsBuilder.Assert(async manager => await manager.CreateAsync(person), Is.SameAs(repositoryResult));

                        builder.Test();
                    }
                });
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

            [Test]
            public void ReturnsOriginalRepositoryExceptionWhenExistsReturnsFalse()
            {
                var person = new Person(Name: "Already exists");
                var originalException = new Exception();
                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(person);

                builder.RepositoryBuilder
                    .Throws(repository => repository.CreateAsync(person), originalException)
                    .Returns(repository => repository.ExistsAsync(person.Name), new IExistsPersonResult.Success(Exists: false));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.CreateAsync(person), Is.EqualTo(new ICreatePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }

            [Test]
            public void ReturnsOriginalRepositoryExceptionWhenExistsReturnsFailure()
            {
                var person = new Person(Name: "Maybe exists");
                var originalException = new Exception();
                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(person);

                builder.RepositoryBuilder
                    .Throws(repository => repository.CreateAsync(person), originalException)
                    .Returns(repository => repository.ExistsAsync(person.Name), new IExistsPersonResult.RepositoryFailure(new Exception()));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.CreateAsync(person), Is.EqualTo(new ICreatePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }

            [Test]
            public void ReturnsOriginalRepositoryExceptionWhenExistsThrows()
            {
                var person = new Person(Name: "Maybe exists");
                var originalException = new Exception();
                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(person);

                builder.RepositoryBuilder
                    .Throws(repository => repository.CreateAsync(person), originalException)
                    .Throws(repository => repository.ExistsAsync(person.Name), new Exception());

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.CreateAsync(person), Is.EqualTo(new ICreatePersonResult.RepositoryFailure(originalException)));

                builder.Test();
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

            [Test]
            public void ReturnsRepositoryResult()
            {
                string name = "Joe";
                var repositoryResults = new IDeletePersonResult[]
                {
                    new IDeletePersonResult.NotFound(),
                    new IDeletePersonResult.RepositoryFailure(new Exception()),
                    new IDeletePersonResult.Success(),
                };

                Assert.Multiple(async () =>
                {
                    foreach (var repositoryResult in repositoryResults)
                    {
                        var builder = new PeopleManagerTestBuilder();
                        builder.RepositoryBuilder.Returns(repository => repository.DeleteAsync(name), repositoryResult);
                        builder.AssertionsBuilder.Assert(async manager => await manager.DeleteAsync(name), Is.SameAs(repositoryResult));
                        builder.Test();
                    }
                });
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

            [Test]
            public void ReturnsOriginalRepositoryFailureWhenExistsReturnsTrue()
            {
                string name = "Exception thrower";
                var originalException = new Exception();

                var builder = new PeopleManagerTestBuilder();

                builder.RepositoryBuilder
                    .Throws(repository => repository.DeleteAsync(name), originalException)
                    .Returns(repository => repository.ExistsAsync(name), new IExistsPersonResult.Success(Exists: true));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.DeleteAsync(name), Is.EqualTo(new IDeletePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }

            [Test]
            public void ReturnsOriginalRepositoryFailureWhenExistsReturnsFailure()
            {
                string name = "Exception thrower";
                var originalException = new Exception();

                var builder = new PeopleManagerTestBuilder();

                builder.RepositoryBuilder
                    .Throws(repository => repository.DeleteAsync(name), originalException)
                    .Returns(repository => repository.ExistsAsync(name), new IExistsPersonResult.RepositoryFailure(new Exception()));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.DeleteAsync(name), Is.EqualTo(new IDeletePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }

            [Test]
            public void ReturnsOriginalRepositoryFailureWhenExistsThrows()
            {
                string name = "Exception thrower";
                var originalException = new Exception();

                var builder = new PeopleManagerTestBuilder();

                builder.RepositoryBuilder
                    .Throws(repository => repository.DeleteAsync(name), originalException)
                    .Throws(repository => repository.ExistsAsync(name), new Exception());

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.DeleteAsync(name), Is.EqualTo(new IDeletePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }
        }

        [TestFixture]
        public class UpdateTests
        {
            [Test]
            public void ThrowsForNullPerson()
            {
                var builder = new PeopleManagerTestBuilder();
                builder.AssertionsBuilder.Assert(async manager => await manager.UpdateAsync(null!), Throws.TypeOf<ArgumentNullException>());
                builder.Test();
            }

            [Test]
            public void ReturnsRepositoryResult()
            {
                var personToUpdate = new Person("Joe");
                var updatedPerson = new Person("Updated Joe");

                var repositoryResults = new IUpdatePersonResult[]
                {
                    new IUpdatePersonResult.Conflict(),
                    new IUpdatePersonResult.NotFound(),
                    new IUpdatePersonResult.RepositoryFailure(new Exception()),
                    new IUpdatePersonResult.Success(updatedPerson),
                };

                Assert.Multiple(() =>
                {
                    foreach (var repositoryResult in repositoryResults)
                    {
                        var builder = new PeopleManagerTestBuilder();

                        builder.ValidatorBuilder.ValidatesPerson(personToUpdate);
                        builder.RepositoryBuilder.Returns(repository => repository.UpdateAsync(personToUpdate), repositoryResult);
                        builder.AssertionsBuilder.Assert(async manager => await manager.UpdateAsync(personToUpdate), Is.SameAs(repositoryResult));

                        builder.Test();
                    }
                });
            }

            [Test]
            public void DetectsMissingPersonManually()
            {
                var personToUpdate = new Person(Name: "Not found");

                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(personToUpdate);

                builder.RepositoryBuilder
                    .Throws(repository => repository.UpdateAsync(personToUpdate), new Exception())
                    .Returns(repository => repository.ExistsAsync(personToUpdate.Name), new IExistsPersonResult.Success(Exists: false));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.UpdateAsync(personToUpdate), Is.EqualTo(new IUpdatePersonResult.NotFound()));

                builder.Test();
            }

            [Test]
            public void DetectsConflictingPersonManually()
            {
                var personToUpdate = new Person(Name: "Not found");

                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(personToUpdate);

                builder.RepositoryBuilder
                    .Throws(repository => repository.UpdateAsync(personToUpdate), new Exception())
                    .Returns(repository => repository.ExistsAsync(personToUpdate.Name), new IExistsPersonResult.Success(Exists: true));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.UpdateAsync(personToUpdate), Is.EqualTo(new IUpdatePersonResult.Conflict()));

                builder.Test();
            }

            [Test]
            public void ReturnsOriginalRepositoryFailureWhenExistsReturnsFailure()
            {
                var personToUpdate = new Person(Name: "Maybe exists");
                var originalException = new Exception();
                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(personToUpdate);

                builder.RepositoryBuilder
                    .Throws(repository => repository.UpdateAsync(personToUpdate), originalException)
                    .Returns(repository => repository.ExistsAsync(personToUpdate.Name), new IExistsPersonResult.RepositoryFailure(new Exception()));

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.UpdateAsync(personToUpdate), Is.EqualTo(new IUpdatePersonResult.RepositoryFailure(originalException)));

                builder.Test();
            }

            [Test]
            public void ReturnsOriginalRepositoryFailureWhenExistsThrows()
            {
                var personToUpdate = new Person(Name: "Maybe exists");
                var originalException = new Exception();
                var builder = new PeopleManagerTestBuilder();

                builder.ValidatorBuilder.ValidatesPerson(personToUpdate);

                builder.RepositoryBuilder
                    .Throws(repository => repository.UpdateAsync(personToUpdate), originalException)
                    .Throws(repository => repository.ExistsAsync(personToUpdate.Name), new Exception());

                builder.AssertionsBuilder
                    .Assert(async manager => await manager.UpdateAsync(personToUpdate), Is.EqualTo(new IUpdatePersonResult.RepositoryFailure(originalException)));

                builder.Test();
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
