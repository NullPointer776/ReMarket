using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ReMarket.Models.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RichTextAttribute : ValidationAttribute
    {
        private static readonly Regex BlockedPattern = new(
            @"(?i)(<\s*/?\s*script|<\s*/?\s*iframe|<\s*/?\s*object|<\s*/?\s*embed|javascript\s*:|vbscript\s*:|data\s*:\s*text/html|on\w+\s*=)",
            RegexOptions.Compiled);

        public RichTextAttribute()
            : base("Description contains invalid or unsafe content.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not string text || string.IsNullOrWhiteSpace(text))
                return ValidationResult.Success;

            if (BlockedPattern.IsMatch(text))
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
