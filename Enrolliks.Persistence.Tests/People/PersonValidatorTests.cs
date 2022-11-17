using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Test]
        public void ReturnsNull()
        {
            var validPerson = new Person("Max Caulfield");
            AssertValidationErrors(validPerson, errors: null);
        }

        private static void AssertValidationErrors(Person person, PersonValidationErrors? errors)
        {
            Assert.That(new PersonValidator().Validate(person), Is.EqualTo(errors));
        }

        [TestFixture]
        public class NameTests
        {
            // This method is here to ensure that if any additional state is added to Person, all properties but the name will be valid.
            private static Person BuildPerson(string name)
            {
                return new Person(name);
            }

            private static void AssertNameValidationError<TError>(string name, TError error) where TError : IPersonNameValidationError
            {
                AssertValidationErrors(BuildPerson(name), new PersonValidationErrors { Name = error });
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
