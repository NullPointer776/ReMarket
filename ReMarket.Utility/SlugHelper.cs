using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReMarket.Utility
{
    /// <summary>
    /// Builds URL-friendly slugs from display text (ASCII letters and digits; other characters become hyphens).
    /// </summary>
    public static class SlugHelper
    {
        /// <summary>
        /// Converts text to a lowercase slug suitable for routes. Returns "item" if the result would be empty.
        /// </summary>
        /// <param name="text">Source string (e.g. item or category title).</param>
        public static string ToSlug(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "item";

            var normalized = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc == UnicodeCategory.NonSpacingMark)
                    continue;
                if (char.IsLetterOrDigit(c))
                    sb.Append(c);
                else if (char.IsWhiteSpace(c) || c is '-' or '_')
                    sb.Append('-');
            }

            var s = sb.ToString();
            while (s.Contains("--", StringComparison.Ordinal))
                s = s.Replace("--", "-", StringComparison.Ordinal);
            s = s.Trim('-');
            return string.IsNullOrEmpty(s) ? "item" : s;
        }
    }
}
