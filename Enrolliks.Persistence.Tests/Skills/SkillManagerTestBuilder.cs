using Enrolliks.Persistence.Skills;
using NUnit.Framework;

namespace Enrolliks.Persistence.Tests.Skills
{
    internal class SkillManagerTestBuilder
    {
        public MockBuilder<ISkillsRepository> RepositoryBuilder { get; } = new();
        public MockBuilder<ISkillValidator> ValidatorBuilder { get; } = new();
        public AssertionsBuilder<SkillsManager> AssertionsBuilder { get; } = new();

        public void Test()
        {
            var (repository, verifyRepository) = RepositoryBuilder.Build();
            var (validator, verifyValidator) = ValidatorBuilder.Build();
            var manager = new SkillsManager(repository, validator);

            Assert.Multiple(() =>
            {
                AssertionsBuilder.Test(manager);
                Assert.That(verifyRepository, Throws.Nothing);
                Assert.That(verifyValidator, Throws.Nothing);
            });
        }
    }
}
