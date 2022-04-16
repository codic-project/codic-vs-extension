using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodicExtension
{
    internal static class StringExtensions
    {
        public static string Truncate(this string value, int maxChars, string postfix = "...")
        {
            if (string.IsNullOrEmpty(value))
                return value;
            var result = value.Length <= maxChars ? value : value.Substring(0, maxChars) + postfix;
            result = result.Replace(Environment.NewLine, "");
            return result;
        }

    }
}