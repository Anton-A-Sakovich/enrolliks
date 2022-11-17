using System;
using System.Text.RegularExpressions;

namespace Enrolliks.Persistence.Skills
{
    internal class SkillValidator : ISkillValidator
    {
        private const int _minCharactersRequired = 3;
        private const int _maxCharactersAllowed = 128;

        private static readonly Regex _disallowedLettersRegex = new(@"[^A-Za-z0-9\.\-\ ]", RegexOptions.Compiled);

        public SkillValidationErrors? Validate(Skill skill)
        {
            if (skill is null) throw new ArgumentNullException(nameof(skill));

            ISkillNameValidationError? nameError = skill.Name switch
            {
                var s when string.IsNullOrWhiteSpace(s) => new ISkillNameValidationError.Empty(),

                string s when s.Length < _minCharactersRequired => new ISkillNameValidationError.TooShort(_minCharactersRequired),
                string s when s.Length > _maxCharactersAllowed => new ISkillNameValidationError.TooLong(_maxCharactersAllowed),

                string s when _disallowedLettersRegex.Match(s) is Match { Success: true } match
                    => new ISkillNameValidationError.DisallowedLetter(s.Substring(match.Index, match.Length)),

                _ => null
            };

            if (nameError is not null)
                return new SkillValidationErrors { Name = nameError };

            return null;
        }
    }
}
