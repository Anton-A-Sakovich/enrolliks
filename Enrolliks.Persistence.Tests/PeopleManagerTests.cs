using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Enrolliks.Persistence.Tests
{
    [TestFixture]
    public class PeopleManagerTests
    {
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

            [TestCase(null)]
            [TestCase("")]
            [TestCase(" ")]
            public async Task ReturnsValidationErrorForEmptyName(string name)
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var manager = new PeopleManager(repository.Object);
                var person = new Person(name);

                var actual = await manager.CreateAsync(person);
                var expected = new ICreatePersonResult.ValidationFailure(new PersonValidationErrors
                {
                    Name = new INameValidationError.Empty()
                });

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyNoOtherCalls, Throws.Nothing);
                });
            }

            [TestCase("a")]
            [TestCase("ab")]
            public async Task ReturnsValidationErrorForShortName(string name)
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var manager = new PeopleManager(repository.Object);
                var person = new Person(name);

                var actual = await manager.CreateAsync(person);
                var expected = new ICreatePersonResult.ValidationFailure(new PersonValidationErrors
                {
                    Name = new INameValidationError.TooShort(MinCharactersRequired: 3)
                });

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyNoOtherCalls, Throws.Nothing);
                });
            }

            [TestCaseSource(nameof(GetLongNames))]
            public async Task ReturnsValidationErrorLongName(string name)
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var manager = new PeopleManager(repository.Object);
                var person = new Person(name);

                var actual = await manager.CreateAsync(person);
                var expected = new ICreatePersonResult.ValidationFailure(new PersonValidationErrors
                {
                    Name = new INameValidationError.TooLong(MaxCharactersAllowed: 128)
                });

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyNoOtherCalls, Throws.Nothing);
                });
            }

            private static IEnumerable<object[]> GetLongNames()
            {
                yield return new object[] { GetLongString(129) };
                yield return new object[] { GetLongString(130) };
            }

            private static string GetLongString(int length)
            {
                int firstLetter = 'a';
                int lettersCount = 'z' - 'a' + 1;
                var chars = Enumerable.Range(0, length).Select(i => (char)(firstLetter + (i % lettersCount)));
                return string.Concat(chars);
            }

            [Test]
            public void ReturnsRepositoryResult()
            {
                var person = new Person(Name: "Mary Shepherd-Sunderland");
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
                        await ReturnsRepositoryResult(person, repositoryResult);
                    }
                });
            }

            private static async Task ReturnsRepositoryResult(Person person, ICreatePersonResult repositoryResult)
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                repository.Setup(r => r.CreateAsync(person)).ReturnsAsync(repositoryResult);

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.CreateAsync(person);
                var expected = repositoryResult;

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.SameAs(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
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
            public async Task FallsBackToRepositoryFailureWhenExistsReturnsFalse()
            {
                await FallsBackToRepositoryFailureWhen(exists => exists.ReturnsAsync(new IExistsPersonResult.Success(Exists: false)));
            }

            [Test]
            public async Task FallsBackToRepositoryFailureWhenExistsReturnsFailure()
            {
                await FallsBackToRepositoryFailureWhen(exists => exists.ReturnsAsync(new IExistsPersonResult.RepositoryFailure(new Exception())));
            }

            [Test]
            public async Task FallsBackToRepositoryFailureWhenExistsThrows()
            {
                await FallsBackToRepositoryFailureWhen(exists => exists.ThrowsAsync(new Exception()));
            }

            private static async Task FallsBackToRepositoryFailureWhen(Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsReturns)
            {
                var person = new Person(Name: "Mary Shepherd-Sunderland");

                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var createException = new Exception();
                repository.Setup(r => r.CreateAsync(person)).ThrowsAsync(createException);
                existsReturns(repository.Setup(r => r.ExistsAsync(person.Name)));

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.CreateAsync(person);
                var expected = new ICreatePersonResult.RepositoryFailure(createException);

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
                });
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
                string name = "Mary Shepherd-Sunderland";
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
                        await ReturnsRepositoryResult(name, repositoryResult);
                    }
                });
            }

            private static async Task ReturnsRepositoryResult(string name, IDeletePersonResult repositoryResult)
            {
                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                repository.Setup(r => r.DeleteAsync(name)).ReturnsAsync(repositoryResult);

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.DeleteAsync(name);
                var expected = repositoryResult;

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.SameAs(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
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
            public async Task FallsBackToRepositoryFailureWhenExistsReturnsTrue()
            {
                await FallsBackToRepositoryFailureWhen(exists => exists.ReturnsAsync(new IExistsPersonResult.Success(Exists: true)));
            }

            [Test]
            public async Task FallsBackToRepositoryFailureWhenExistsReturnsFailure()
            {
                await FallsBackToRepositoryFailureWhen(exists => exists.ReturnsAsync(new IExistsPersonResult.RepositoryFailure(new Exception())));
            }

            [Test]
            public async Task FallsBackToRepositoryFailureWhenExistsThrows()
            {
                await FallsBackToRepositoryFailureWhen(exists => exists.ThrowsAsync(new Exception()));
            }

            private static async Task FallsBackToRepositoryFailureWhen(Action<Moq.Language.Flow.ISetup<IPeopleRepository, Task<IExistsPersonResult>>> existsReturns)
            {
                string name = "Mary Shepherd-Sunderland";

                var repository = new Mock<IPeopleRepository>(MockBehavior.Strict);
                var deleteException = new Exception();
                repository.Setup(r => r.DeleteAsync(name)).ThrowsAsync(deleteException);
                existsReturns(repository.Setup(r => r.ExistsAsync(name)));

                var manager = new PeopleManager(repository.Object);

                var actual = await manager.DeleteAsync(name);
                var expected = new IDeletePersonResult.RepositoryFailure(deleteException);

                Assert.Multiple(() =>
                {
                    Assert.That(actual, Is.EqualTo(expected));
                    Assert.That(repository.VerifyAll, Throws.Nothing);
                });
            }
        }
    }
}
