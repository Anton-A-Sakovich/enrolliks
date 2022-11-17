using System.Linq;

namespace Enrolliks.Persistence.Tests
{
    internal static class StringGenerator
    {
        public static string GenerateLongString(int length)
        {
            int firstLetter = 'a';
            int lettersCount = 'z' - 'a' + 1;
            var chars = Enumerable.Range(0, length).Select(i => (char)(firstLetter + (i % lettersCount)));
            return string.Concat(chars);
        }
    }
}
