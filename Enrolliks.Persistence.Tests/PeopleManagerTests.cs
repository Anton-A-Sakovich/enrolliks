using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Enrolliks.Persistence.Tests
{
    [TestFixture]
    public class PeopleManagerTests
    {
        private static async Task ReturnsRepositoryResult<TResult>(TResult repositoryResult,
            Expression<Func<IPeopleRepository, Task<TResult>>> repositoryMethod,
            Func<PeopleManager, Task<TResult>> managerMethod)
        {
            var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
            repository.Setup(repositoryMethod).ReturnsAsync(repositoryResult);

            var manager = new PeopleManager(repository.Object);

            var actual = await managerMethod.Invoke(manager);
            var expected = repositoryResult;

            Assert.Multiple(() =>
            {
                Assert.That(actual, Is.SameAs(expected));
                Assert.That(repository.VerifyAll, Throws.Nothing);
            });
        }

        private static async Task ReturnsOriginalRepositoryFailure<TResult, TFailedResult>(
            Func<Person, Func<PeopleManager, Task<TResult>>> managerMethod,
            Func<Person, Expression<Func<IPeopleRepository, Task<TResult>>>> repositoryMethodToThrow,
            Func<Person, Action<Mock<IPeopleRepository>>> additionalRepositorySetup,
            Func<Exception, TFailedResult> failureFactory
            )
            where TFailedResult : TResult
        {
            var person = new Person(Name: "Joe");

            var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);

            var originalException = new Exception();
            repository.Setup(repositoryMethodToThrow(person)).ThrowsAsync(originalException);
            additionalRepositorySetup(person)(repository);

            var manager = new PeopleManager(repository.Object);

            var actual = await managerMethod(person)(manager);
            var expected = failureFactory(originalException);

            Assert.Multiple(() =>
            {
                Assert.That(actual, Is.EqualTo(expected));
                Assert.That(repository.VerifyAll, Throws.Nothing);
            });
        }

        public class ValidationTests
        {
            public static IEnumerable<object?[]> GetEmptyNames()
            {
                return new object?[][]
                {
                    new object?[] { null },
                    new object?[] { "" },
                    new object?[] { " " },
                };
            }

            public static IEnumerable<object?[]> GetShortNames()
            {
                return new object?[][]
                {
                    new object?[] { "a" },
                    new object?[] { "ab" },
                };
            }

            public static IEnumerable<object?[]> GetLongNames()
            {
                yield return new object?[] { GetLongString(129) };
                yield return new object?[] { GetLongString(130) };
            }

            private static string GetLongString(int length)
            {
                int firstLetter = 'a';
                int lettersCount = 'z' - 'a' + 1;
                var chars = Enumerable.Range(0, length).Select(i => (char)(firstLetter + (i % lettersCount)));
                return string.Concat(chars);
            }

            public static async Task ReturnsValidationErrorForEmptyName<TResult>(string name,
                Func<IPeopleManager, Person, Task<TResult>> managerMethod,
                Func<PersonValidationErrors, TResult> resultFactory)
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var manager = new PeopleManager(repository.Object);
                var person = new Person(name);

                var actual = await managerMethod.Invoke(manager, person);
                var expected = resultFactory.Invoke(new PersonValidationErrors
                {
                    Name = new INameValidationError.Empty()
                });

                Assert.That(actual, Is.EqualTo(expected));
            }

            public static async Task ReturnsValidationErrorForShortName<TResult>(string name,
                Func<IPeopleManager, Person, Task<TResult>> managerMethod,
                Func<PersonValidationErrors, TResult> resultFactory)
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var manager = new PeopleManager(repository.Object);
                var person = new Person(name);

                var actual = await managerMethod.Invoke(manager, person);
                var expected = resultFactory.Invoke(new PersonValidationErrors
                {
                    Name = new INameValidationError.TooShort(MinCharactersRequired: 3)
                });

                Assert.That(actual, Is.EqualTo(expected));
            }

            public static async Task ReturnsValidationErrorLongName<TResult>(string name,
                Func<IPeopleManager, Person, Task<TResult>> managerMethod,
                Func<PersonValidationErrors, TResult> resultFactory)
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var manager = new PeopleManager(repository.Object);
                var person = new Person(name);

                var actual = await managerMethod.Invoke(manager, person);
                var expected = resultFactory.Invoke(new PersonValidationErrors
                {
                    Name = new INameValidationError.TooLong(MaxCharactersAllowed: 128)
                });

                Assert.That(actual, Is.EqualTo(expected));
            }
        }

        [TestFixture]
        public class CreateTests
        {
            [Test]
            public void ThrowsForNullPerson()
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var manager = new PeopleManager(repository.Object);
                Person person = null!;

                Assert.Multiple(() =>
                {
                    Assert.That(async () => await manager.CreateAsync(person), Throws.Exception.TypeOf<ArgumentNullException>());
                    Assert.That(repository.VerifyNoOtherCalls, Throws.Nothing);
                });
            }

            [TestCaseSource(typeof(ValidationTests), nameof(ValidationTests.GetEmptyNames))]
            public async Task ReturnsValidationErrorForEmptyName(string name)
            {
                await ValidationTests.ReturnsValidationErrorForEmptyName(name,
                    (manager, person) => manager.CreateAsync(person),
                    validationErrors => new ICreatePersonResult.ValidationFailure(validationErrors));
            }

            [TestCaseSource(typeof(ValidationTests), nameof(ValidationTests.GetShortNames))]
            public async Task ReturnsValidationErrorForShortName(string name)
            {
                await ValidationTests.ReturnsValidationErrorForShortName(name,
                    (manager, person) => manager.CreateAsync(person),
                    validationErrors => new ICreatePersonResult.ValidationFailure(validationErrors));
            }

            [TestCaseSource(typeof(ValidationTests), nameof(ValidationTests.GetLongNames))]
            public async Task ReturnsValidationErrorLongName(string name)
            {
                await ValidationTests.ReturnsValidationErrorLongName(name,
                    (manager, person) => manager.CreateAsync(person),
                    validationErrors => new ICreatePersonResult.ValidationFailure(validationErrors));
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

                Assert.Multiple(async () =>
                {
                    foreach (var repositoryResult in repositoryResults)
                    {
                        await PeopleManagerTests.ReturnsRepositoryResult(repositoryResult,
                            repository => repository.CreateAsync(person),
                            manager => manager.CreateAsync(person));
                    }
                });
            }

            [Test]
            public async Task DetectsConflictManually()
            {
                var person = new Person(Name: "Already exists");

                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                repository.Setup(r => r.CreateAsync(person)).ThrowsAsync(new Exception());
                repository.Setup(r => r.ExistsAsync(person.Name)).ReturnsAsync(new IExistsPersonResult.Success(Exists: true));

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.CreateAsync(person);
                var expected = new ICreatePersonResult.Conflict();

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
                });
            }

            [Test]
            public async Task ReturnsOriginalRepositoryFailureWhenExistsReturnsFalse()
            {
                await ReturnsOriginalRepositoryFailure<ICreatePersonResult, ICreatePersonResult.RepositoryFailure>(
                    person => manager => manager.CreateAsync(person),
                    person => repository => repository.CreateAsync(person),
                    person => repository => repository.Setup(r => r.ExistsAsync(person.Name)).ReturnsAsync(new IExistsPersonResult.Success(Exists: false)),
                    exception => new ICreatePersonResult.RepositoryFailure(exception));
            }

            [Test]
            public async Task ReturnsOriginalRepositoryFailureWhenExistsReturnsFailure()
            {
                await ReturnsOriginalRepositoryFailure<ICreatePersonResult, ICreatePersonResult.RepositoryFailure>(
                    person => manager => manager.CreateAsync(person),
                    person => repository => repository.CreateAsync(person),
                    person => repository => repository.Setup(r => r.ExistsAsync(person.Name)).ReturnsAsync(new IExistsPersonResult.RepositoryFailure(new Exception())),
                    exception => new ICreatePersonResult.RepositoryFailure(exception));
            }

            [Test]
            public async Task ReturnsOriginalRepositoryFailureWhenExistsThrows()
            {
                await ReturnsOriginalRepositoryFailure<ICreatePersonResult, ICreatePersonResult.RepositoryFailure>(
                    person => manager => manager.CreateAsync(person),
                    person => repository => repository.CreateAsync(person),
                    person => repository => repository.Setup(r => r.ExistsAsync(person.Name)).ThrowsAsync(new Exception()),
                    exception => new ICreatePersonResult.RepositoryFailure(exception));
            }
        }

        [TestFixture]
        public class DeleteTests
        {
            [Test]
            public void ThrowsForNullName()
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var manager = new PeopleManager(repository.Object);
                string name = null!;

                Assert.That(async () => await manager.DeleteAsync(name), Throws.TypeOf<ArgumentNullException>());
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
                        await PeopleManagerTests.ReturnsRepositoryResult(repositoryResult,
                            repository => repository.DeleteAsync(name),
                            manager => manager.DeleteAsync(name));
                    }
                });
            }

            [Test]
            public async Task DetectsMissingPersonManually()
            {
                string name = "Not found";

                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                repository.Setup(r => r.DeleteAsync(name)).ThrowsAsync(new Exception());
                repository.Setup(r => r.ExistsAsync(name)).ReturnsAsync(new IExistsPersonResult.Success(Exists: false));

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.DeleteAsync(name);
                var expected = new IDeletePersonResult.NotFound();

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
                });
            }

            [Test]
            public async Task ReturnsOriginalRepositoryFailureWhenExistsReturnsTrue()
            {
                await ReturnsOriginalRepositoryFailure<IDeletePersonResult, IDeletePersonResult.RepositoryFailure>(
                    person => manager => manager.DeleteAsync(person.Name),
                    person => repository => repository.DeleteAsync(person.Name),
                    person => repository => repository.Setup(r => r.ExistsAsync(person.Name)).ReturnsAsync(new IExistsPersonResult.Success(Exists: true)),
                    exception => new IDeletePersonResult.RepositoryFailure(exception));
            }

            [Test]
            public async Task ReturnsOriginalRepositoryFailureWhenExistsReturnsFailure()
            {
                await ReturnsOriginalRepositoryFailure<IDeletePersonResult, IDeletePersonResult.RepositoryFailure>(
                    person => manager => manager.DeleteAsync(person.Name),
                    person => repository => repository.DeleteAsync(person.Name),
                    person => repository => repository.Setup(r => r.ExistsAsync(person.Name)).ReturnsAsync(new IExistsPersonResult.RepositoryFailure(new Exception())),
                    exception => new IDeletePersonResult.RepositoryFailure(exception));
            }

            [Test]
            public async Task ReturnsOriginalRepositoryFailureWhenExistsThrows()
            {
                await ReturnsOriginalRepositoryFailure<IDeletePersonResult, IDeletePersonResult.RepositoryFailure>(
                    person => manager => manager.DeleteAsync(person.Name),
                    person => repository => repository.DeleteAsync(person.Name),
                    person => repository => repository.Setup(r => r.ExistsAsync(person.Name)).ThrowsAsync(new Exception()),
                    exception => new IDeletePersonResult.RepositoryFailure(exception));
            }
        }

        [TestFixture]
        public class UpdateTests
        {
            [Test]
            public void ThrowsForNullPerson()
            {
                Person person = null!;
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var manager = new PeopleManager(repository.Object);

                Assert.That(async () => await manager.UpdateAsync(person), Throws.TypeOf<ArgumentNullException>());
            }

            [TestCaseSource(typeof(ValidationTests), nameof(ValidationTests.GetEmptyNames))]
            public async Task ReturnsValidationErrorForEmptyName(string name)
            {
                await ValidationTests.ReturnsValidationErrorForEmptyName(name,
                    (manager, person) => manager.UpdateAsync(person),
                    validationErrors => new IUpdatePersonResult.ValidationFailure(validationErrors));
            }

            [TestCaseSource(typeof(ValidationTests), nameof(ValidationTests.GetShortNames))]
            public async Task ReturnsValidationErrorForShortName(string name)
            {
                await ValidationTests.ReturnsValidationErrorForShortName(name,
                    (manager, person) => manager.UpdateAsync(person),
                    validationErrors => new IUpdatePersonResult.ValidationFailure(validationErrors));
            }

            [TestCaseSource(typeof(ValidationTests), nameof(ValidationTests.GetLongNames))]
            public async Task ReturnsValidationErrorLongName(string name)
            {
                await ValidationTests.ReturnsValidationErrorLongName(name,
                    (manager, person) => manager.UpdateAsync(person),
                    validationErrors => new IUpdatePersonResult.ValidationFailure(validationErrors));
            }

            [Test]
            public void ReturnsRepositoryResult()
            {
                var originalPerson = new Person("Joe");
                var updatedPerson = new Person("Joe1");

                var repositoryResults = new IUpdatePersonResult[]
                {
                    new IUpdatePersonResult.Conflict(),
                    new IUpdatePersonResult.NotFound(),
                    new IUpdatePersonResult.RepositoryFailure(new Exception()),
                    new IUpdatePersonResult.Success(updatedPerson),
                };

                Assert.Multiple(async () =>
                {
                    foreach (var repositoryResult in repositoryResults)
                    {
                        await PeopleManagerTests.ReturnsRepositoryResult(repositoryResult,
                            repository => repository.UpdateAsync(originalPerson),
                            manager => manager.UpdateAsync(originalPerson));
                    }
                });
            }

            [Test]
            public async Task DetectsMissingPersonManually()
            {
                var originalPerson = new Person(Name: "Not found.");

                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                repository.Setup(r => r.UpdateAsync(originalPerson)).ThrowsAsync(new Exception());
                repository.Setup(r => r.ExistsAsync(originalPerson.Name)).ReturnsAsync(new IExistsPersonResult.Success(Exists: false));

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.UpdateAsync(originalPerson);
                var expected = new IUpdatePersonResult.NotFound();

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
                });
            }

            [Test]
            public async Task DetectsConflictingPersonManually()
            {
                var originalPerson = new Person(Name: "Not found.");

                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                repository.Setup(r => r.UpdateAsync(originalPerson)).ThrowsAsync(new Exception());
                repository.Setup(r => r.ExistsAsync(originalPerson.Name)).ReturnsAsync(new IExistsPersonResult.Success(Exists: true));

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.UpdateAsync(originalPerson);
                var expected = new IUpdatePersonResult.Conflict();

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
                });
            }

            [Test]
            public async Task ReturnsOriginalRepositoryFailureWhenExistsReturnsFailure()
            {
                await ReturnsOriginalRepositoryFailure<IUpdatePersonResult, IUpdatePersonResult.RepositoryFailure>(
                    person => manager => manager.UpdateAsync(person),
                    person => repository => repository.UpdateAsync(person),
                    person => repository => repository.Setup(r => r.ExistsAsync(person.Name)).ReturnsAsync(new IExistsPersonResult.RepositoryFailure(new Exception())),
                    exception => new IUpdatePersonResult.RepositoryFailure(exception));
            }

            [Test]
            public async Task ReturnsOriginalRepositoryFailureWhenExistsThrows()
            {
                await ReturnsOriginalRepositoryFailure<IUpdatePersonResult, IUpdatePersonResult.RepositoryFailure>(
                    person => manager => manager.UpdateAsync(person),
                    person => repository => repository.UpdateAsync(person),
                    person => repository => repository.Setup(r => r.ExistsAsync(person.Name)).ThrowsAsync(new Exception()),
                    exception => new IUpdatePersonResult.RepositoryFailure(exception));
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

                Assert.Multiple(async () =>
                {
                    foreach (var repositoryResult in repositoryResults)
                    {
                        await PeopleManagerTests.ReturnsRepositoryResult(repositoryResult,
                            repository => repository.GetAllAsync(),
                            manager => manager.GetAllAsync());
                    }
                });
            }

            [Test]
            public async Task ReturnsRepositoryFailure()
            {
                var repository = new Mock<IPeopleRepository>();

                var exception = new Exception();
                repository.Setup(r => r.GetAllAsync()).ThrowsAsync(exception);

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.GetAllAsync();
                var expected = new IGetAllPeopleResult.RepositoryFailure(exception);

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
                });
            }
        }

        [TestFixture]
        public class ExistsTests
        {
            [Test]
            public void ThrowsForNullName()
            {
                string name = null!;
                var repository = new Mock<IPeopleRepository>();
                var manager = new PeopleManager(repository.Object);

                Assert.Multiple(() =>
                {
                    Assert.That(async () => await manager.ExistsAsync(name), Throws.TypeOf<ArgumentNullException>());
                    Assert.That(repository.VerifyAll, Throws.Nothing);
                });
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

                Assert.Multiple(async () =>
                {
                    foreach (var repositoryResult in repositoryResults)
                    {
                        await PeopleManagerTests.ReturnsRepositoryResult(repositoryResult,
                            repository => repository.ExistsAsync(name),
                            manager => manager.ExistsAsync(name));
                    }
                });
            }

            [Test]
            public async Task ReturnsRepositoryFailure()
            {
                string name = "Joe";
                var repository = new Mock<IPeopleRepository>();

                var exception = new Exception();
                repository.Setup(r => r.ExistsAsync(name)).ThrowsAsync(exception);

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.ExistsAsync(name);
                var expected = new IExistsPersonResult.RepositoryFailure(exception);

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
                });
            }
        }
    }
}
