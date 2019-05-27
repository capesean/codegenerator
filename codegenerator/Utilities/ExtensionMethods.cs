using System;
using System.Linq;
using System.Text;

namespace WEB
{
    public static class ExtensionMethods
    {
        public static string Hyphenated(this string input)
        {
            var regex = new System.Text.RegularExpressions.Regex("[A-Z]");
            return regex.Replace(input.ToCamelCase(), " $&").Trim().Replace(" ", "-").ToLower();
        }

        public static string ToCamelCase(this string input)
        {
            if (input.All(char.IsUpper)) return input.ToLower();
            if (input.Length < 2) return input.ToLower();
            if (char.IsLower(input, 1)) return input.Substring(0, 1).ToLower() + input.Substring(1);

            var result = string.Empty;
            for (var i = 0; i < input.Length; i++)
            {
                var nextIsUpper = i == input.Length - 1 ? true : char.IsUpper(input, i + 1);
                if (nextIsUpper) result += input.Substring(i, 1).ToLower();
                else
                {
                    result += input.Substring(i);
                    break;
                }
            }
            return result;
        }

        public static void Add(this StringBuilder s, string text)
        {
            s.AppendLine(text);//.Replace("    ", "\t"));
        }
    }
}