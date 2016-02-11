using System;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace CodicExtension
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Restores the beginning and end of string eliminated by trim
        /// using the start and the end of the reference string.
        /// </summary>
        /// <param name="trimmedText">Trimmed string.</param>
        /// <param name="originalText">Reference string.</param>
        /// <returns></returns>
        internal static string TrimRestore(this string trimmedText, string originalText)
        {
            if (string.IsNullOrWhiteSpace(trimmedText) || string.IsNullOrEmpty(originalText))
                return trimmedText;

            var length = originalText.Length;
            var firstChar = length - originalText.TrimStart().Length;
            if (firstChar > -1)
            {
                trimmedText = originalText.Substring(0, firstChar) + trimmedText;
            }

            var startIndex = originalText.TrimEnd().Length;
            var lastChar = length - startIndex;
            if (lastChar > -1)
            {
                trimmedText += originalText.Substring(startIndex, lastChar);
            }

            return trimmedText;
        }

        public static string Truncate(this string value, int maxChars, string postfix = "...")
        {
            if (string.IsNullOrEmpty(value))
                return value;
            var result = value.Length <= maxChars ? value : value.Substring(0, maxChars) + postfix;
            result = result.Replace(Environment.NewLine, "");
            return result;
        }

        internal static string ToSingleWordsSpace(this string text)
        {
            return Regex.Replace(text, @"\s+", " ");
        }
    }

    //internal static class VsShellExtensions
    //{
    //    internal static bool IsPackageInstalled(this IVsShell vsShell, Guid packageId)
    //    {
    //        int isInstalled;
    //        return ErrorHandler.Succeeded(vsShell.IsPackageInstalled(ref packageId, out isInstalled)) && 1 == isInstalled;
    //    }
    //}
}