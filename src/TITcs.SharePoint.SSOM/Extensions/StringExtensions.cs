using System.Text.RegularExpressions;

namespace TITcs.SharePoint.SSOM.Extensions
{
    public static class StringExtensions
    {
        public static string Left(this string text, int length)
        {
            return Microsoft.VisualBasic.Strings.Left(text, length);
        }

        public static string Right(this string text, int length)
        {
            return Microsoft.VisualBasic.Strings.Right(text, length);
        }

        public static bool IsNumeric(this string value)
        {
            return Microsoft.VisualBasic.Information.IsNumeric(value);
        }

        public static string RemoveTagsHtml(this string text)
        {
            string result = Regex.Replace(text, @"<[^>]*>", "");

            if (result != "")
                text = result;

            return text;
        }
    }
}
