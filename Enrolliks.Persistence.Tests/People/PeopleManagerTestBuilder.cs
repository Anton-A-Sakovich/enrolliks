using Enrolliks.Persistence.People;
using NUnit.Framework;

namespace Enrolliks.Persistence.Tests.People
{
    internal class PeopleManagerTestBuilder
    {
        public MockBuilder<IPeopleRepository> RepositoryBuilder { get; } = new();
        public MockBuilder<IPersonValidator> ValidatorBuilder { get; } = new();
        public AssertionsBuilder<PeopleManager> AssertionsBuilder { get; } = new();

        public void Test()
        {
            var (repository, verifyRepository) = RepositoryBuilder.Build();
            var (validator, verifyValidator) = ValidatorBuilder.Build();
            var manager = new PeopleManager(repository, validator);

            Assert.Multiple(() =>
            {
                AssertionsBuilder.Test(manager);
                Assert.That(verifyRepository, Throws.Nothing);
                Assert.That(verifyValidator, Throws.Nothing);
            });
        }
    }
}
