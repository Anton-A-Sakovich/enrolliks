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

        private static void AssertSkillValidationErrors(Skill skill, SkillValidationErrors? expectedErrors)
        {
            Assert.That(new SkillValidator().Validate(skill), Is.EqualTo(expectedErrors));
        }

        [TestFixture]
        public class NameTests
        {
            private static void AssertNameValidationError(string? name, ISkillNameValidationError? expectedNameError)
            {
                var expectedSkillErrors = expectedNameError switch
                {
                    ISkillNameValidationError nameError => new SkillValidationErrors { Name = nameError },
                    _ => null,
                };

                AssertSkillValidationErrors(new Skill(Id: "1", Name: name!), expectedSkillErrors);
            }

            [TestCase("ABC")]
            [TestCase("abc")]
            [TestCase("123")]
            [TestCase(" .-")]
            public void ReturnsNull(string name)
            {
                AssertNameValidationError(name, null);
            }

            [TestCase((object)null!)]
            [TestCase("")]
            [TestCase(" ")]
            [TestCase("  ")]
            public void ReturnsEmptyResult(string? name)
            {
                AssertNameValidationError(name, new ISkillNameValidationError.Empty());
            }

            [TestCase("a")]
            [TestCase("ab")]
            public void ReturnsTooShortResult(string name)
            {
                AssertNameValidationError(name, new ISkillNameValidationError.TooShort(MinCharactersRequired: 3));
            }

            [TestCaseSource(nameof(GetLongNames))]
            public void ReturnsTooLongResult(string name)
            {
                AssertNameValidationError(name, new ISkillNameValidationError.TooLong(MaxCharactersAllowed: 128));
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
                AssertNameValidationError(name, new ISkillNameValidationError.DisallowedLetter(expectedOffendingLetter));
            }
        }
    }
}
