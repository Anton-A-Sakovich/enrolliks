using System;
using System.Collections.Generic;
using Enrolliks.Persistence.People;
using NUnit.Framework;

namespace Enrolliks.Persistence.Tests.People
{
    [TestFixture]
    public class PersonValidatorTests
    {
        [Test]
        public void ThrowsArgumentNullException()
        {
            var validator = new PersonValidator();
            Assert.That(() => validator.Validate(null!), Throws.TypeOf<ArgumentNullException>());
        }

        private static void AssertPersonValidationErrors(Person person, PersonValidationErrors? expectedErrors)
        {
            Assert.That(new PersonValidator().Validate(person), Is.EqualTo(expectedErrors));
        }

        [TestFixture]
        public class NameTests
        {
            private static void AssertNameValidationError(string? name, IPersonNameValidationError? expectedNameError)
            {
                var expectedPersonErrors = expectedNameError switch
                {
                    IPersonNameValidationError nameError => new PersonValidationErrors { Name = nameError },
                    _ => null,
                };

                AssertPersonValidationErrors(new Person(Name: name!), expectedPersonErrors);
            }

            [TestCase("Max")]
            [TestCase("Max Caulfield")]
            public void ReturnsNull(string name)
            {
                AssertNameValidationError(name, null);
            }

            [TestCase(new object[] { null! })]
            [TestCase("")]
            [TestCase(" ")]
            [TestCase("  ")]
            public void ReturnsEmpty(string name)
            {
                AssertNameValidationError(name, new IPersonNameValidationError.Empty());
            }

            [TestCase("a")]
            [TestCase("ab")]
            [TestCase(" a")]
            public void ReturnsTooShort(string name)
            {
                AssertNameValidationError(name, new IPersonNameValidationError.TooShort(3));
            }

            [TestCaseSource(nameof(GetLongNames))]
            public void ReturnsTooLong(string name)
            {
                AssertNameValidationError(name, new IPersonNameValidationError.TooLong(128));
            }

            private static IEnumerable<object[]> GetLongNames()
            {
                yield return new object[] { StringGenerator.GenerateLongString(129) };
                yield return new object[] { StringGenerator.GenerateLongString(130) };
            }

            [TestCase(" Max")]
            public void ReturnsStartsWithSpace(string name)
            {
                AssertNameValidationError(name, new IPersonNameValidationError.StartsWithSpace());
            }

            [TestCase("Max!", "!")]
            [TestCase("$Max", "$")]
            [TestCase("/Max/", "/")]
            public void ReturnsDisallowedLetter(string name, string expectedOffendingLetter)
            {
                AssertNameValidationError(name, new IPersonNameValidationError.DisallowedLetter(expectedOffendingLetter));
            }
        }
    }
}
