using System.Text.RegularExpressions;

namespace MMR.DevTool
{
    public static class TextMutate
    {
        public static readonly Regex AddSpacesRegex = new Regex("(?<!^)([A-Z])");

        public static string AddSpaces(string input)
        {
            return AddSpacesRegex.Replace(input, " $1");
        }
    }
}
