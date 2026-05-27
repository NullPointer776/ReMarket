using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ReMarket.Models.Validation
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class SafeTextAttribute : ValidationAttribute
    {
        private static readonly Regex HtmlTagPattern = new(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex UnsafePattern = new(
            @"(javascript\s*:|vbscript\s*:|data\s*:\s*text/html|on\w+\s*=|<\s*/?\s*script)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public SafeTextAttribute()
            : base("Input contains invalid or unsafe characters.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string text || string.IsNullOrWhiteSpace(text))
                return ValidationResult.Success;

            if (text.Contains('<') || text.Contains('>'))
                return new ValidationResult(ErrorMessage);

            if (HtmlTagPattern.IsMatch(text) || UnsafePattern.IsMatch(text))
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
