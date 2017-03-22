using System;
using System.Linq;
using System.Text;

namespace WEB
{
    public static class ExtensionMethods
    {
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
                else {
                    result += input.Substring(i);
                    break;
                }
            }
            return result;

            //var result = string.Empty;
            //var lastWasUpper = true;
            //var atLeastOneLower = false;
            //for (var i = 0; i < input.Length; i++)
            //{
            //    var isUpper = char.IsUpper(input, i);
            //    if (!isUpper) atLeastOneLower = true;
            //    var nextIsUpper = i == input.Length - 1 ? true : char.IsUpper(input, i + 1);

            //    if (i == 0) result += input.Substring(i, 1).ToLower(); // first = lower
            //    else if (lastWasUpper && isUpper && nextIsUpper && !atLeastOneLower) result += input.Substring(i, 1).ToLower(); // e.g. the C in KCAText => kcaText
            //    else if (lastWasUpper && isUpper && nextIsUpper && atLeastOneLower) result += input.Substring(i, 1).ToUpper(); // e.g. the T in AssessmentDocumentDTO => assessmentDocumentDTO
            //    //else if (i == name.Length - 1) result += name.Substring(i, 1).ToLower(); // last = lower
            //    else if (!isUpper) result += input.Substring(i, 1).ToLower(); // lower = lower
            //    else if (!lastWasUpper) result += input.Substring(i, 1).ToUpper(); // upper after lower = upper
            //    else if (nextIsUpper) result += input.Substring(i, 1).ToUpper(); // upper after upper, before upper = upper
            //    else result += input.Substring(i, 1).ToUpper(); // upper after upper, before lower = upper

            //    lastWasUpper = isUpper;
            //}

            //return result;
        }

        public static void Add(this StringBuilder s, string text)
        {
            s.AppendLine(text);//.Replace("    ", "\t"));
        }
    }
}