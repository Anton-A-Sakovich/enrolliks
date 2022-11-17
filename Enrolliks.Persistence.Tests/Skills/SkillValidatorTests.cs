using System;
using System.Collections.Generic;
using Enrolliks.Persistence.Skills;
using NUnit.Framework;

namespace Enrolliks.Persistence.Tests.Skills
{
    [TestFixture]
    public class SkillValidatorTests
    {
        [Test]
        public void ThrowsForNullSkill()
        {
            var validator = new SkillValidator();
            Assert.That(() => validator.Validate(skill: null!), Throws.TypeOf<ArgumentNullException>());
        }

        [TestCase((object)null!)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase("  ")]
        public void ReturnsEmptyResult(string? name)
        {
            var skill = new Skill(Id: "1", name!);
            var validator = new SkillValidator();
            var expected = new SkillValidationErrors
            {
                Name = new ISkillNameValidationError.Empty(),
            };

            var actual = validator.Validate(skill);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCase("a")]
        [TestCase("ab")]
        public void ReturnsTooShortResult(string name)
        {
            var skill = new Skill(Id: "1", name);
            var validator = new SkillValidator();
            var expected = new SkillValidationErrors
            {
                Name = new ISkillNameValidationError.TooShort(MinCharactersRequired: 3),
            };

            var actual = validator.Validate(skill);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [TestCaseSource(nameof(GetLongNames))]
        public void ReturnsTooLongResult(string name)
        {
            var skill = new Skill(Id: "1", name);
            var validator = new SkillValidator();
            var expected = new SkillValidationErrors
            {
                Name = new ISkillNameValidationError.TooLong(MaxCharactersAllowed: 128),
            };

            var actual = validator.Validate(skill);

            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<object[]> GetLongNames()
        {
            yield return new object[] { StringGenerator.GenerateLongString(129) };
            yield return new object[] { StringGenerator.GenerateLongString(130) };
        }

        [TestCase("Hello!", "!")]
        [TestCase("/Hello", "/")]
        [TestCase("[Hello]", "[")]
        [TestCase("Hel*llo", "*")]
        public void ReturnsDisallowedLetterResult(string name, string expectedOffendingLetter)
        {
            var skill = new Skill(Id: "1", name);
            var validator = new SkillValidator();
            var expected = new SkillValidationErrors
            {
                Name = new ISkillNameValidationError.DisallowedLetter(expectedOffendingLetter),
            };

            var actual = validator.Validate(skill);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
