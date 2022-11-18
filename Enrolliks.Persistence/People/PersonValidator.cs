using System;
using System.Text.RegularExpressions;

namespace Enrolliks.Persistence.People
{
    internal class PersonValidator : IPersonValidator
    {
        private const int _minNameLength = 3;
        private const int _maxNameLength = 128;

        private static readonly Regex _disallowedLetterRegex = new(@"[^A-Za-z\ ]", RegexOptions.Compiled);

        public PersonValidationErrors? Validate(Person person)
        {
            if (person is null) throw new ArgumentNullException(nameof(person));

            IPersonNameValidationError? nameError = person.Name switch
            {
                var s when string.IsNullOrWhiteSpace(s) => new IPersonNameValidationError.Empty(),

                string s when s.Length < _minNameLength => new IPersonNameValidationError.TooShort(_minNameLength),
                string s when s.Length > _maxNameLength => new IPersonNameValidationError.TooLong(_maxNameLength),

                string s when char.IsWhiteSpace(s, 0) => new IPersonNameValidationError.StartsWithSpace(),

                string s when _disallowedLetterRegex.Match(s) is Match { Success: true } match
                    => new IPersonNameValidationError.DisallowedLetter(s.Substring(match.Index, match.Length)),

                _ => null,
            };

            if (nameError is not null)
                return new PersonValidationErrors { Name = nameError };

            return null;
        }
    }
}
